module GameOfCogs

open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi
open Fable.Core.JsInterop
open Fable.Import.Animejs
open Fable.AnimeUtils
open Types 


let DisplayParticles (model:GameScreen.CogModel) delta = 
  model.Emitters
    |> Seq.iter( fun emitter -> emitter.update (delta * 0.01) )
  
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
      

// displays a "great" message
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

        let prepareAnimation scale= 
          jsOptions<AnimInput> (fun o ->
            o.elasticity <- !!100.
            o.duration <- !!500.
            o.targets <- !!sprite.scale
            o.Item("x") <- scale
            o.Item("y") <- scale
          )
        
        // create our tweening timeline
        let timeline = GetTimeline None
        
        // prepare our animations
        [
          prepareAnimation 1.0 // scale in 
          prepareAnimation 0.0 // scale out
        ] |> Seq.iter( fun options -> timeline.add options |> ignore ) 
      
    | None -> failwith "no layer top"

      
let handleMessage (model:GameScreen.CogModel) (msg:GameScreen.Cog.Msg option) = 

    if msg.IsSome then 
      match msg.Value with 
      | GameScreen.Cog.OnMove (cog,pointerId) -> 
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
                    Cog.onDragEnd cog () |> ignore
                    
                    // display the target cog
                    Cog.show target |> ignore                      
                    
                    // update score
                    score <- score + 1

                    // sound feedback 
                    Fable.SoundUtils.play "goodMove"

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

        model.Targets <- updatedTargets 
        model.Score <- score 
        model.Found <- found 
        model.Emitters <- emitters

let playAnimations model delta = 
  // update our particles
  DisplayParticles model delta

  // Animations
  if model.Score > 0 then

    // make our cogs turn
    for i in 0..(model.Found.Length-1) do
      let index = model.Found.[i]
      let target = model.Targets.[index] 

      // animate all the cogs that have been found
      // make sure the next cog will turn on the opposite direction
      let way = if i % 2 = 0 then 1. else -1.0

      // smaller cogs run faster
      let speed = Cog.rotation * (1.25 - (Cog.scaleFactor target.Data.Size))
      target.rotation <- target.rotation + way * speed
                 
let Update (model:GameScreen.CogModel option) stage (renderer:PIXI.WebGLRenderer) delta =

  let model, moveToNextScreen =
    match model with 

    // this is a brand new model
    | None -> 

      let sizes = Cog.cogSizes()
      let newModel : GameScreen.CogModel = 
        {
          Cogs = [||] 
          Targets = [||]
          Score= 0
          Goal=sizes.Length
          State=GameScreen.CogState.Init
          Found=[||]
          Emitters=[||]
          Sizes = sizes
          Layers = ["ui";"targets";"cogs";"dock";"emitter";"top"]
        }
      newModel, false

    // update our model
    | Some model ->  
      match model.State with 
        | GameScreen.CogState.MoveToNextScreen ->
          model, true

        | GameScreen.CogState.Win ->

          Fable.SoundUtils.play "winSound"

          // display a lovely win anim
          let winSprite = 
            SpriteUtils.fromTexture "win"
            |> SpriteUtils.addToLayer "top"
            |> SpriteUtils.scaleTo 0. 0.
            |> SpriteUtils.moveTo (renderer.width * 0.5) (renderer.height * 0.5)
            |> SpriteUtils.setAnchor 0.5 0.5            

          let inAnim = Fable.AnimeUtils.XY winSprite.scale 1.0 1.0 300. 1200.
          let outAnim = Fable.AnimeUtils.XY winSprite.scale 0. 0. 300. 0.
          outAnim.delay <- !!2000. // wait 2 seconds 

          // create our tweening timeline
          let timelineOptions = 
            jsOptions<AnimInput>( fun o -> 
              o.complete <- 
                fun _ -> model.State <- GameScreen.CogState.MoveToNextScreen
            )
          
          let timeline = GetTimeline (Some timelineOptions)
          
          // prepare our animations using a timeline
          // each animation will play once and one after the other
          [
            inAnim // simple scale in 
            outAnim // simple scale in 
          ] |> Seq.iter( fun options -> timeline.add options |> ignore ) 
      
          model.State <- GameScreen.CogState.DoNothing
          model, false

        | GameScreen.CogState.Init ->           

            // add our layers
            model.Layers
              |> List.iter( fun name -> Layers.add name stage |> ignore ) 
        
            model.State <- GameScreen.CogState.PlaceHelp
            model,false      

        | GameScreen.CogState.PlaceHelp -> 
          
          SpriteUtils.fromTexture "help1"
            |> SpriteUtils.addToLayer "ui"
            |> SpriteUtils.moveTo (renderer.width * 0.55) (renderer.height * 0.75)
            |> SpriteUtils.setAnchor 0.5 0.5          
            |> ignore

          SpriteUtils.fromTexture "help2"
            |> SpriteUtils.addToLayer "ui"
            |> SpriteUtils.moveTo (renderer.width * 0.65) (renderer.height * 0.30)
            |> SpriteUtils.setAnchor 0.5 0.5          
            |> ignore
          
          model.State <- GameScreen.CogState.PlaceCogs
          model,false      

        | GameScreen.CogState.PlaceCogs -> 

          // create our cogs and center them!
          let targets = 
            
            // create our cogs
            // they have to fit in the given space
            let maxWidth = renderer.width * 0.8
            let targets,(totalWidth,totalHeight) 
              = Cog.fitCogInSpace model 0 (0.,0.) (0.,0.) None [] maxWidth model.Sizes
                      
            // center our cogs
            let xMargin = (renderer.width - totalWidth) * 0.5
            let yMargin = totalHeight * 0.5
            targets 
              |> Seq.map ( 
                  (Cog.placeMarker xMargin yMargin (renderer.height*0.5) "cogs") 
                  >> Cog.castTo
                  )
               |> Seq.toArray
         
          model.Targets <- targets
          model.State <- GameScreen.CogState.PlaceDock
          model.Goal <- targets.Length
                    
          model,false     
                
        | GameScreen.CogState.PlaceDock -> // prepare our 4 base cogs

          let container = Layers.get "dock"
          match container with 
            | Some c -> 
              let cogs =  
                Dock.prepare c stage renderer 
                |> Seq.map( fun cog -> 
                    cog.interactive <- true
                    cog.buttonMode <- true        
                    cog
                      |> Fable.Pixi.Event.attach Fable.Pixi.Event.Pointerdown (
                        fun iev ->
                          // sound feedback 
                          Fable.SoundUtils.play "startDrag"                         
                          Cog.onDragStart cog iev)

                      |> Fable.Pixi.Event.attach Fable.Pixi.Event.Pointerup (Cog.onDragEnd cog)
                      |> Fable.Pixi.Event.attach Fable.Pixi.Event.Pointermove (Cog.onDragMove (handleMessage model) stage cog)
                      |> c.addChild
                      |> Cog.castTo
                )
                |> Seq.toArray

              model.Cogs <- cogs
              model.State <- GameScreen.CogState.Play
            | None -> failwith "unknown container dock"

          model,false
        | GameScreen.CogState.Play -> 
          
          playAnimations model delta

          if model.Score >=  model.Goal then       
            model.State <- GameScreen.CogState.Win
            model,false
          else
            model,false

        | GameScreen.CogState.DoNothing -> 
          
          playAnimations model delta
          model,false

  model, moveToNextScreen 