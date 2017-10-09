module ScreenIntroduction

open Types 
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.Animejs

let getEmptyModel() = 
  {
    State=IntroductionState.Init
  }

// displays a "great" message
let titleAnim x y (container:PIXI.Container) model (renderer:PIXI.WebGLRenderer) = 

  let handleClick model (renderer:PIXI.WebGLRenderer) _ =
    renderer.plugins.interaction.on( 
      !!string Pointerdown,
      (fun _ -> 
        printfn "click"
        model.State <- IntroductionState.NextScreen) ) |> ignore
  
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
        o.complete <- Some (handleClick model renderer) 
      )
    
    // create our tweening timeline
    let timeline = anime.Globals.timeline()
    
    // prepare our animations
    [
      prepareTitleAnimation 1.0 // simple scale in 
      prepareSubTitleAnimation 1.0 // simple scale in 
    ] |> Seq.iter( fun options -> timeline.add options |> ignore ) 

     
let Update model nextScreen stage (renderer:PIXI.WebGLRenderer) delta =
  
  match model.State with 
    
    | IntroductionState.NextScreen -> 
      printfn "next"
      nextScreen

    | IntroductionState.Init -> 
      titleAnim (renderer.width * 0.5) (renderer.height * 0.5) stage model renderer
      
      ScreenKind.Introduction (
          { model with State= IntroductionState.DoNothing}
          ,nextScreen)

    | IntroductionState.Play -> 
      ScreenKind.Introduction (model,nextScreen)           
    
    | IntroductionState.DoNothing -> 
      ScreenKind.Introduction (model,nextScreen) 
     