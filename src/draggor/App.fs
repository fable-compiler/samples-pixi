module App

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.Pixi.Particles
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

// our layers 
// markers layer
let targetsContainer = PIXI.Container()
app.stage.addChild targetsContainer |> ignore

// found cogs layer
let cogsContainer = PIXI.Container()
app.stage.addChild cogsContainer |> ignore
  
// dragging cogs layer
let dockContainer = PIXI.Container()
app.stage.addChild dockContainer |> ignore
  
let emitterContainer = PIXI.Container()
app.stage.addChild(emitterContainer) |> ignore


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
    let emitter = PIXI.particles.Emitter( emitterContainer, !![|texture.Value|], config )
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
          State=PlaceCogs}

      | PlaceCogs -> 

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
                  >> targetsContainer.addChild
                )
             |> Seq.toArray
       
        { model with Targets = targets; State=PlaceDock }
      
      | PlaceDock -> // prepare our 4 base cogs

        let cogs = Dock.prepare dockContainer app.stage renderer 
        { model with Cogs=cogs; State=Play}

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

  // when all is loaded, start our render loop
  let onLoaded (loader:PIXI.loaders.Loader) (res:PIXI.loaders.Resource) =
    
    // fill our Asset store 
    Assets.addTexture "help1" !!res?help1?texture 
    Assets.addTexture "help2" !!res?help2?texture 
    Assets.addTexture "cog" !!res?cog?texture 
    Assets.addTexture "target" !!res?target?texture 
    Assets.addTexture "particle" !!res?particle?texture 

    // our particle configuration file 
    Assets.addObj "rightConfig" !!res?rightConfig?data
    Assets.addObj "leftConfig" !!res?leftConfig?data

    // start our loop
    app.ticker.add tick |> ignore

  // We start by loading the emitter json configuration File
  // to get our particle animation parameters
  // This json is built using pixi particles online editor 
  // you can find the editor here: http://pixijs.github.io/pixi-particles-editor/
  let loader = PIXI.loaders.Loader()
  loader.add("rightConfig", "../img/draggor/right.json") |> ignore
  loader.add("leftConfig", "../img/draggor/left.json") |> ignore
  loader.add("help1", "../img/draggor/help1.png") |> ignore
  loader.add("help2", "../img/draggor/help2.png") |> ignore
  loader.add("particle", "../img/draggor/particle.png") |> ignore
  loader.add("cog", "../img/draggor/cog.png") |> ignore
  loader.add("target", "../img/draggor/target.png") |> ignore
  loader.load(onLoaded) |> ignore

start() // it all begins there
