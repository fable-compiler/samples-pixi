module ScreenIntroduction

open Types 
open Fable.Import
open Fable.Import.Browser
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.Animejs
open Fable.Import.Pixi.Sound

[<Literal>]
let MAX_COGS = 300

[<Literal>]
let TIMEOUT = 300.


let titleAnim texts handleClick scaleTo  = 

    let (s1:PIXI.Sprite),(s2:PIXI.Sprite) = texts

    let duration = 700.
    let elasticity = 300.

    let prepareTitleAnimation scale= 
      AnimationUtils.XY s1.scale scale scale duration elasticity

    let prepareSubTitleAnimation scale= 
      let options = AnimationUtils.XY s2.scale scale scale duration elasticity
      options.complete <- Some handleClick 
      options

    // create our tweening timeline
    let timeline = anime.Globals.timeline()
    
    // prepare our animations using a timeline
    // each animation will play once and one after the other
    [
      prepareTitleAnimation scaleTo // simple scale in 
      prepareSubTitleAnimation scaleTo // simple scale in 
    ] |> Seq.iter( fun options -> timeline.add options |> ignore ) 


let Update (model:IntroductionScreen.Model option) (stage:PIXI.Container) (renderer:PIXI.WebGLRenderer) delta =
  
  let model, moveToNextScreen =
    match model with 

    // this is a brand new model
    | None -> 

      let newModel : IntroductionScreen.Model = 
        {
          State=IntroductionScreen.Init
          Texts=None
          Layers = ["cogs";"root";]
          CogList=[||]
          Id=0.
        }
      newModel, false

    // update our model
    | Some model -> 
      match model.State with 
        
        | IntroductionScreen.MoveToNextScreen -> 
          model,true

        | IntroductionScreen.Init -> 

          let prepareSprites x y (container:PIXI.Container) = 

            let titleSprite = 
              SpriteUtils.fromTexture "title"
              |> SpriteUtils.addToLayer "root"
              |> SpriteUtils.scaleTo 0. 0.
              |> SpriteUtils.moveTo x y
              |> SpriteUtils.setAnchor 0.5 0.5

            let subSprite = 
              SpriteUtils.fromTexture "subtitle"
              |> SpriteUtils.addToLayer "root"
              |> SpriteUtils.scaleTo 0. 0.
              |> SpriteUtils.moveTo x (y+120.) 
              |> SpriteUtils.setAnchor 0.5 0.5
            
            titleSprite,subSprite

          // add our layers
          model.Layers
            |> List.iter( fun name -> Layers.add name stage |> ignore ) 

          // we handle all clicks happening on screen
          let handleClick (renderer:PIXI.WebGLRenderer) _ =
            renderer.plugins.interaction.on( 
              !!string Pointerdown,
              (fun _ -> 
                model.State <- IntroductionScreen.State.ByeBye
               ) ) |> ignore
        
          // start our animations
          let texts =
            let container = Layers.get "root"
            match container with
            | Some c->   
              prepareSprites (renderer.width * 0.5) (renderer.height * 0.4) c 
            | None -> failwith "unkown container root"
          
          model.Texts <- Some texts 
          
          let scaleTo = 1.0
          titleAnim texts (handleClick renderer) scaleTo
          
          // add a new cog every second
          let addCog (model:IntroductionScreen.Model) _ =

            let randomScale = JS.Math.random() * 0.8

            // set our custom data
            let angle =  if JS.Math.random()  > 0.5 then -1. else 1.
            let data : IntroductionScreen.CustomSprite = {Angle=angle}

            let texture = Assets.getTexture "cog"
            if texture.IsSome then 
              let castTo (sprite: PIXI.Sprite) = 
                sprite :?> ExtendedSprite<IntroductionScreen.CustomSprite>              

              let cog = 
                ExtendedSprite(texture.Value,data)
                |> SpriteUtils.addToLayer "cogs"
                |> SpriteUtils.scaleTo randomScale randomScale
                |> SpriteUtils.moveTo (renderer.width * JS.Math.random()) (renderer.height * JS.Math.random())
                |> SpriteUtils.setAnchor 0.5 0.5          
                |> SpriteUtils.setAlpha randomScale          
                |> castTo

              model.CogList <- [|model.CogList;[|cog|]|] |> Array.concat

              if model.CogList.Length >= MAX_COGS then
                window.clearInterval model.Id

          // our spawn function
          model.Id <- window.setInterval( (addCog model), TIMEOUT)

          model.State <- IntroductionScreen.Play        
          model,false

        | IntroductionScreen.ByeBye ->       

          let onComplete _ =
            model.State <- IntroductionScreen.State.MoveToNextScreen

          let scaleTo = 0.0
          match model.Texts with 
          | None -> failwith ("Can't play animations on non existing texts!")
          | Some texts -> titleAnim texts onComplete scaleTo

          model.State <- IntroductionScreen.EndAnim
          
          // remove our timeout
          window.clearTimeout model.Id
          
          model,false 

        | IntroductionScreen.EndAnim ->
          
          // fade all our cogs fast
          for i in 0..(model.CogList.Length-1) do
            let cog = model.CogList.[i]
            cog.alpha <- cog.alpha - 0.05

          model,false 

        | IntroductionScreen.Play ->
          
          // let our cogs rotate
          for i in 0..(model.CogList.Length-1) do
            let cog = model.CogList.[i]
            let scale : PIXI.Point = !!cog.scale            
            let speed = ( 0.8 - scale.x ) * 0.1

            cog.rotation <- speed * cog.Data.Angle + cog.rotation

          model,false 
          
        | IntroductionScreen.DoNothing -> 
          model,false 

  model, moveToNextScreen 