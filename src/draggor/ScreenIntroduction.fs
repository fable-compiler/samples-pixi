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

let prepareSprites x y (container:PIXI.Container) = 
  
  let help = Assets.getTexture "title"
  let sub = Assets.getTexture "subtitle"
  if help.IsSome && sub.IsSome then 

    let titleSprite = 
      PIXI.Sprite help.Value
      |> container.addChild    
    titleSprite._anchor.set 0.5

    let scale : PIXI.Point = !!titleSprite.scale
    scale.x <- 0.0
    scale.y <- 0.0

    let position : PIXI.Point = !!titleSprite.position
    position.x <- x
    position.y <- y

    let subSprite = 
      PIXI.Sprite sub.Value
      |> container.addChild    
    subSprite._anchor.set 0.5

    let scale : PIXI.Point = !!subSprite.scale
    scale.x <- 0.0
    scale.y <- 0.0

    let position : PIXI.Point = !!subSprite.position
    position.x <- x
    position.y <- y + 120.

    titleSprite,subSprite
  
  else failwith("failed to create sprites, textures are not ready yet")


let titleAnim texts handleClick scaleTo  = 

    let (s1:PIXI.Sprite),(s2:PIXI.Sprite) = texts

    let prepareTitleAnimation scale= 
      jsOptions<anime.AnimeAnimParams> (fun o ->
        o.elasticity <- !!300.
        o.duration <- !!700.
        o.targets <- !!s1.scale
        o.Item("x") <- scale
        o.Item("y") <- scale
      )

    let prepareSubTitleAnimation scale= 
      jsOptions<anime.AnimeAnimParams> (fun o ->
        o.elasticity <- !!300.
        o.duration <- !!700.
        o.targets <- !!s2.scale
        o.Item("x") <- scale
        o.Item("y") <- scale
        
        // when the second animation is complete, add our click event
        o.complete <- Some handleClick 
      )
    
    // create our tweening timeline
    let timeline = anime.Globals.timeline()
    
    // prepare our animations
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
              prepareSprites (renderer.width * 0.5) (renderer.height * 0.5) c 
            | None -> failwith "unkown container root"
          
          model.Texts <- Some texts 
          
          let scaleTo = 1.0
          titleAnim texts (handleClick renderer) scaleTo
          
          // add a new cog every second
          let addCog (model:IntroductionScreen.Model) _ =
            let texture = Assets.getTexture "cog"
            let container = Layers.get "cogs"
            if texture.IsSome && container.IsSome then 

              let castTo (sprite: PIXI.Sprite) = 
                sprite :?> ExtendedSprite<IntroductionScreen.CustomSprite>              

              let angle =  if JS.Math.random()  > 0.5 then -1. else 1.
              let data : IntroductionScreen.CustomSprite = {Angle=angle}
              let cog = 
                ExtendedSprite(texture.Value,data)
                |> container.Value.addChild    
                |> castTo    
              
              cog.anchor.set 0.5

              let scale : PIXI.Point = !!cog.scale
              let factor = JS.Math.random() * 0.8
              scale.x <- factor
              scale.y <- factor

              // alpha is relative to size
              cog.alpha <- factor
              
              let position : PIXI.Point = !!cog.position
              position.x <- renderer.width * JS.Math.random()
              position.y <- renderer.height * JS.Math.random()         
              model.CogList <- [|model.CogList;[|cog|]|] |> Array.concat

              if model.CogList.Length >= MAX_COGS then
                window.clearInterval model.Id

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
          
          // fade all our cogs
          for i in 0..(model.CogList.Length-1) do
            let cog = model.CogList.[i]
            cog.alpha <- cog.alpha - 0.1

          model,false 

        | IntroductionScreen.Play ->
          
          // let our cogs rotate
          for i in 0..(model.CogList.Length-1) do
            let cog = model.CogList.[i]
            let scale : PIXI.Point = !!cog.scale            
            let speed = ( 1.25 - scale.x ) * 0.1

            cog.rotation <- speed * cog.Data.Angle + cog.rotation

          model,false 
          
        | IntroductionScreen.DoNothing -> 
          model,false 

  model, moveToNextScreen 