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
  o.antialias <- Some true
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let renderer : PIXI.WebGLRenderer = !!app.renderer


let startLoop() = 

  let mutable model = {
    Screen=ScreenKind.GameOfCogs
  }

  let mutable cogsModel =
    {
      Cogs = [||] 
      Targets = [||]
      Score= 0
      Goal=0
      State=Init
      Found=[||]
      Emitters=[||]
    }

  app.ticker.add (fun delta -> 

    model <- 
      match model.Screen with 
      | ScreenKind.GameOver -> model
      | ScreenKind.Introduction -> model
      | ScreenKind.GameOfCogs ->  
        let nextScreen = GameOfCogs.Update cogsModel app.stage renderer delta
        { model with Screen = nextScreen } 

    ) |> ignore 

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

    // create our screen layers 
    Layers.add "cogs" app.stage |> ignore
    
    // create our layers
    Layers.add "ui" app.stage |> ignore
    Layers.add "targets" app.stage |> ignore
    Layers.add "cogs" app.stage |> ignore
    Layers.add "dock" app.stage |> ignore
    Layers.add "emitter" app.stage |> ignore      
    Layers.add "top" app.stage |> ignore

    startLoop()
    // create our starting Model
  ) |> ignore

start() // it all begins there
