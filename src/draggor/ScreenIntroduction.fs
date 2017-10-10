module ScreenIntroduction

open Types 
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.Animejs


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

     
let Clean (model:IntroductionScreen.Model) = 

  if model.Root.IsSome then 
    let layer = model.Root.Value
    
    // remove children
    layer.children
      |> Seq.iteri( fun i child -> 
        layer.removeChild( layer.children.[i] ) |> ignore
      )        
    // remove layer from parent
    layer.parent.removeChild layer |> ignore

     
let Update (model:IntroductionScreen.Model option) (stage:PIXI.Container) (renderer:PIXI.WebGLRenderer) delta =
  
  let model, moveToNextScreen =
    match model with 

    // this is a brand new model
    | None -> 

      let newModel : IntroductionScreen.Model = 
        {
          State=IntroductionScreen.Init
          Texts=None
          Root=Some (stage.addChild (PIXI.Container()))
        }
      newModel, false

    // update our model
    | Some model -> 
      match model.State with 
        
        | IntroductionScreen.MoveToNextScreen -> 
          model,true

        | IntroductionScreen.Init -> 

          // we handle all clicks happening on screen
          let handleClick (renderer:PIXI.WebGLRenderer) _ =
            renderer.plugins.interaction.on( 
              !!string Pointerdown,
              (fun _ -> 
                model.State <- IntroductionScreen.State.ByeBye
               ) ) |> ignore
        
          // start our animations
          let texts = 
            prepareSprites (renderer.width * 0.5) (renderer.height * 0.5) model.Root.Value 
          
          model.Texts <- Some texts 
          
          let scaleTo = 1.0
          titleAnim texts (handleClick renderer) scaleTo
          
          model.State <- IntroductionScreen.Play        
          model,false

        | IntroductionScreen.ByeBye ->       

          // we modify the state to move to the next screen
          let onComplete _ =
            model.State <- IntroductionScreen.State.MoveToNextScreen

          let scaleTo = 0.0
          match model.Texts with 
          | None -> failwith ("Can't play animations on non existing texts!")
          | Some texts -> titleAnim texts onComplete scaleTo

          model.State <- IntroductionScreen.Play
          model,false 

        | IntroductionScreen.Play ->       
          model,false 
          
        | IntroductionScreen.DoNothing -> 
          model,false 

  model, moveToNextScreen 