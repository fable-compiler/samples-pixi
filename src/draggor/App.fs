module App

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.Pixi.Particles
open Fable.Import.Animejs
open Fable.Pixi
open Elmish
open Hink
open Types
open Cog

// our view
let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let renderer : PIXI.WebGLRenderer = !!app.renderer

let greatAnim x y = 

  let container = Layers.get "top"
  match container with 
    | Some c -> 
      let help = Assets.getTexture "great"
      if help.IsSome then 
        let sprite = 
          PIXI.Sprite help.Value
          |> c.addChild
        
        sprite._anchor.set 0.5

        let scale : PIXI.Point = !!sprite.scale
        scale.x <- 0.0
        scale.y <- 0.0

        let position : PIXI.Point = !!sprite.position
        position.x <- x
        position.y <- y

                
        let timelineOptions = 
          jsOptions<anime.AnimeTimelineInstance>( fun o -> 
            o.complete <- fun _ -> sprite.parent.removeChild(sprite) |> ignore
          )

        let prepareAnimation scale= 
          jsOptions<anime.AnimeAnimParams> (fun o ->
            o.elasticity <- !!100.
            o.duration <- !!500.
            o.targets <- !!sprite.scale
            o.Item("x") <- scale
            o.Item("y") <- scale
            o.complete <- Some (fun _ -> printfn "done")
          )
        
        // create our tweening timeline
        let timeline = anime.Globals.timeline(!!timelineOptions)
        
        // prepare our animations
        [
          prepareAnimation 1.0 // scale in 
          prepareAnimation 0.0 // scale out
        ] |> Seq.iter( fun options -> timeline.add options |> ignore ) 
      
    | None -> failwith "no layer top"


let turnCogs model =
  
  model.Found
    |> Seq.iter( fun i ->
      let target = model.Targets.[i] 
      // animate all the cogs that have been found
      // make sure the next cog will turn on the opposite direction
      let way = if i % 2 = 0 then 1. else -1.0
      // smaller cogs run faster
      let speed = Cog.rotation * (1.25 - (Cog.scaleFactor target.Data.Size))
      target.rotation <- target.rotation + way * speed
    )
  
let addEmitter x y config = 
  let texture = Assets.getTexture "particle"
  if texture.IsSome then 
     
    // create our emitter 
    let container = Layers.get "emitter"
    if container.IsNone then
      failwith "unknown layer emitter"
    else 
      let emitter = PIXI.particles.Emitter( container.Value, !![|texture.Value|], config )
      emitter.updateOwnerPos(x,y)

      // start emitting particles
      emitter.emit <- true

      Some emitter
  else 
    None

// our render loop
let tick delta =

  // update our particle systems
  model.Emitters
    |> Seq.iter( fun emitter -> emitter.update (delta * 0.01) )

  model <- 
    match model.State with 
      | Init -> 

        {model with 
          Goal=Cog.cogSizes.Length // the cogs to place correctly in order to win
          State=PlaceHelp}

      | PlaceHelp -> 
        
        let container = Layers.get "ui"
        match container with 
          | Some c -> 
            let help = Assets.getTexture "help1"
            if help.IsSome then 
              let sprite = 
                PIXI.Sprite help.Value
                |> c.addChild

              sprite._anchor.set(0.5) 
              let position : PIXI.Point = !!sprite.position
              position.x <- renderer.width * 0.65
              position.y <- renderer.height * 0.75

            let help = Assets.getTexture "help2"
            if help.IsSome then 
              let sprite = 
                PIXI.Sprite help.Value
                |> c.addChild

              sprite._anchor.set(0.5) 
              let position : PIXI.Point = !!sprite.position
              position.x <- renderer.width * 0.70
              position.y <- renderer.height * 0.3

          | None -> failwith "ui layer not found"

        { model with State =PlaceCogs} 

      | PlaceCogs -> 

        let container = Layers.get "cogs"
        match container with 
          | Some c -> 

            // create our cogs and center them!
            let targets = 
              
              // create our cogs
              // they have to fit in the given space
              let maxWidth = renderer.width * 0.8
              let targets,(totalWidth,totalHeight) 
                = Cog.fitCogInSpace 0 (0.,0.) (0.,0.) None [] maxWidth Cog.cogSizes
                        
              // center our cogs
              let xMargin = (renderer.width - totalWidth) * 0.5
              let yMargin = totalHeight * 0.5
              targets 
                |> Seq.map ( 
                    (Cog.placeMarker xMargin yMargin (renderer.height*0.5)) 
                      >> c.addChild
                    )
                 |> Seq.toArray
           
            { model with Targets = targets; State=PlaceDock }
          | None -> failwith "unknown container cogs"
      
      | PlaceDock -> // prepare our 4 base cogs

        let container = Layers.get "dock"
        match container with 
          | Some c -> 
            let cogs = Dock.prepare c app.stage renderer 
            { model with Cogs=cogs; State=Play}
          | None -> failwith "unknown container dock"

      | Play -> 

        // Animations
        if model.Score > 0 then
          turnCogs model
        
        // Events
        let (updatedTargets,score,foundCogs,emitters) =

          if model.Message.IsSome then 
            let msg = model.Message.Value
            match msg  with 
              | OnMove cog -> // we want to know if we've dragged one of our cog on a target
                
                
                let pos : PIXI.Point = !!cog.position
                let mutable score = model.Score
                let mutable found = model.Found
                let mutable emitters = model.Emitters
                let updatedTargets = 
                  model.Targets
                    |> Seq.mapi( fun i target -> 
                      if not target.Data.IsFound then 
                        let position : PIXI.Point = !!target.position
                        
                        // very simple distance text
                        let a = position.x - pos.x
                        let b = position.y - pos.y
                        let distance = JS.Math.sqrt( a*a + b*b)

                        // look if we are in close vicinity of a potential target
                        let checkRadius = Cog.cogWidth * (Cog.scaleFactor cog.Data.Size) * 0.2
                        if distance < checkRadius then
                          if cog.Data.Size = target.Data.Size then 

                            // ok our cog has been placed at the right place
                            // store index for faster animation renders              
                            found <- Array.append found [|i|]
                            
                            // restore cog to initial position
                            Cog.onDragEnd cog ()
                            
                            // display the target cog
                            Cog.show target |> ignore                      
                            
                            // update score
                            score <- score + 1

                            // add new particle system at the right place
                            // given the way our cog is turning
                            let isLeft = (i % 2 = 0)
                            let config = 
                              if isLeft then 
                                Assets.getObj "leftConfig" 
                              else 
                                Assets.getObj "rightConfig"

                            if config.IsSome then 
                              let config = config.Value
                              let x = 
                                if isLeft then 
                                  position.x - target.width * 0.4
                                else
                                  position.x + target.width * 0.4

                              let y = 
                                  position.y - target.height * 0.3
                                
                              let newEmitter = (addEmitter x y config)
                              if newEmitter.IsSome then 
                                emitters <- Array.append emitters [|newEmitter.Value|]

                              // Great feedback anim
                              greatAnim position.x (position.y - 50.)                                
                        //target
                      target
                    )
                    |> Seq.toArray
                  
                  // return our values
                updatedTargets,score,found, emitters

             else 
              model.Targets, model.Score, model.Found, model.Emitters

        // check if the game's finished
        if model.Score >=  model.Goal then 
          { model with 
              State = GameOver;
              Targets = updatedTargets 
              Found = foundCogs
              Score= score
              Emitters = emitters              
          }
        else 
          { model with 
              Targets = updatedTargets 
              Found = foundCogs
              Score= score              
              Emitters = emitters              
          }

      | DoNothing -> model
      | GameOver -> model

// start our main loop
let start() = 

  // We start by loading our assets 
  let loader = PIXI.loaders.Loader()
  let path = "../img/draggor"
  [
    ("rightConfig",sprintf "%s/right.json" path)
    ("leftConfig",sprintf "%s/left.json" path)
    ("help1",sprintf "%s/help1.png" path)
    ("help2",sprintf "%s/help2.png" path)
    ("particle",sprintf "%s/particle.png" path)
    ("cog",sprintf "%s/cog.png" path)
    ("great",sprintf "%s/great.png" path)
    ("target",sprintf "%s/target.png" path)
  ] 
  |> Seq.iter( fun (name,path) -> loader.add(name,path) |> ignore  )

  loader.load( fun (loader:PIXI.loaders.Loader) (res:PIXI.loaders.Resource) ->
    
    // fill our Asset store 
    Assets.addTexture "help1" !!res?help1?texture 
    Assets.addTexture "help2" !!res?help2?texture 
    Assets.addTexture "cog" !!res?cog?texture 
    Assets.addTexture "target" !!res?target?texture 
    Assets.addTexture "particle" !!res?particle?texture 
    Assets.addTexture "great" !!res?great?texture 

    // our particle configuration file 
    Assets.addObj "rightConfig" !!res?rightConfig?data
    Assets.addObj "leftConfig" !!res?leftConfig?data

    // create our layers
    Layers.add "ui" app.stage |> ignore
    Layers.add "targets" app.stage |> ignore
    Layers.add "cogs" app.stage |> ignore
    Layers.add "dock" app.stage |> ignore
    Layers.add "emitter" app.stage |> ignore      
    Layers.add "top" app.stage |> ignore

    // start our loop
    app.ticker.add tick |> ignore) |> ignore

start() // it all begins there
