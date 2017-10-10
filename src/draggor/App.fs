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

// our view
let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
  o.antialias <- Some true
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let renderer : PIXI.WebGLRenderer = !!app.renderer


let startGame() = 

  // our model is just a tuple composed of a screen and a layer
  (*
  let mutable model =     
    ScreenKind.GameOfCogs (GameOfCogs.getEmptyModel())
    ,Layers.add "gameStage" app.stage

  *)

  let mutable model =    
    ScreenKind.Introduction (ScreenIntroduction.getEmptyModel())
    ,Layers.add "gameStage" app.stage
  
  // our render loop  
  app.ticker.add (fun delta -> 

    model <- 
      match model with 
      | ScreenKind.NextScreen nextScreen, layer -> 

        // do some cleanup before starting the next screen
        layer.children
          |> Seq.iteri( fun i child -> 
            layer.removeChild( layer.children.[i] ) |> ignore
          )        
        layer.parent.removeChild layer |> ignore

        // start next screen
        // add prepare a new layer as well
        (nextScreen,Layers.add "gameStage" app.stage)

      | ScreenKind.GameOver, layer -> model
      
      | ScreenKind.Introduction innerModel, layer -> 
//    let nextScreen = 
//      ScreenKind.NextScreen (ScreenKind.GameOfCogs (GameOfCogs.getEmptyModel())) 
        
        let model = ScreenIntroduction.Update innerModel layer renderer delta
        match model.Msg with 
        | None -> 
          ScreenKind.Introduction (model),layer
        | Some msg ->
          match  msg with          
          | IntroductionScreen.Done -> 
            ScreenKind.NextScreen (ScreenKind.GameOfCogs (GameOfCogs.getEmptyModel())),layer 
          | _ -> 
            ScreenKind.Introduction (model),layer

      
      | ScreenKind.GameOfCogs innerModel, layer ->  
        let model = GameOfCogs.Update innerModel layer renderer delta
        (model,layer)

    ) |> ignore 

// start our main loop
let init() = 

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
    ("title",sprintf "%s/Title.png" path)
    ("subtitle",sprintf "%s/subtitle.png" path)
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
    Assets.addTexture "title" !!res?title?texture 
    Assets.addTexture "subtitle" !!res?subtitle?texture 

    // our particle configuration file 
    Assets.addObj "rightConfig" !!res?rightConfig?data
    Assets.addObj "leftConfig" !!res?leftConfig?data

    // Let's have some fun now!    
    startGame()

  ) |> ignore

init() // it all begins there
