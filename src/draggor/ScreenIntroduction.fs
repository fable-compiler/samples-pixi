module ScreenIntroduction

open Types 
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.Animejs


let getEmptyModel() : IntroductionScreen.Model= 
  {
    State=Types.IntroductionScreen.Init
    Msg=None
  }

// displays a "great" message
let titleAnim handleClick x y (container:PIXI.Container)  = 

  
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

    let prepareTitleAnimation scale= 
      jsOptions<anime.AnimeAnimParams> (fun o ->
        o.elasticity <- !!300.
        o.duration <- !!700.
        o.targets <- !!titleSprite.scale
        o.Item("x") <- scale
        o.Item("y") <- scale
      )

    let prepareSubTitleAnimation scale= 
      jsOptions<anime.AnimeAnimParams> (fun o ->
        o.elasticity <- !!300.
        o.duration <- !!700.
        o.targets <- !!subSprite.scale
        o.Item("x") <- scale
        o.Item("y") <- scale
        
        // when the second animation is complete, add our click event
        o.complete <- Some handleClick 
      )
    
    // create our tweening timeline
    let timeline = anime.Globals.timeline()
    
    // prepare our animations
    [
      prepareTitleAnimation 1.0 // simple scale in 
      prepareSubTitleAnimation 1.0 // simple scale in 
    ] |> Seq.iter( fun options -> timeline.add options |> ignore ) 

     
     
let Update (model:IntroductionScreen.Model) stage (renderer:PIXI.WebGLRenderer) delta =
  

  match model.State with 
    
    | IntroductionScreen.NextScreen -> 
      {model with Msg = Some IntroductionScreen.Done}

    | IntroductionScreen.Init -> 

      // we handle all clicks happening on screen
      let handleClick (renderer:PIXI.WebGLRenderer) _ =
        renderer.plugins.interaction.on( 
          !!string Pointerdown,
          (fun _ -> 
            model.Msg <- Some IntroductionScreen.OnClick
            model.State <- IntroductionScreen.State.NextScreen
           ) ) |> ignore
    
      // start our animations
      titleAnim (handleClick renderer) (renderer.width * 0.5) (renderer.height * 0.5) stage 
      
      model.State <- IntroductionScreen.Play
      model

    | IntroductionScreen.Play ->       
      model 
      
    | IntroductionScreen.DoNothing -> 
      model 
      //ScreenKind.Introduction (model,nextScreen) 
     