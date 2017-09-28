module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

// create a new Sprite from an image path
let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0xAAAACC

let app = PIXI.Application(400., 400., options)
Browser.document.body.appendChild(app.view) |> ignore

let container = PIXI.Container()
app.stage.addChild(container) |> ignore

// create a new Sprite from an image path
let texture = PIXI.Texture.fromImage("../img/fable_logo_small.png")

// Create a 5x5 grid of bunnies
for i in 0..24 do 
  let bunny = PIXI.Sprite(texture)
  bunny.anchor.set(0.5)    
  bunny.x <- float ((i % 5) * 40)
  bunny.y <- Math.floor(float i / 5.) * 40.
  container.addChild(bunny) |> ignore

// Center on the screen
let renderer : PIXI.WebGLRenderer = !!app.renderer
container.x <- renderer.width / 2.
container.y <- renderer.height / 2.

// Center bunny sprite in local container coordinates
let pivot : PIXI.Point = !!container.pivot
pivot.x <- container.width / 2.
pivot.y <- container.height / 2.

// Listen for animate update
app.ticker.add( fun delta -> 
  // rotate the container!
  // use delta to create frame-independent tranform
  container.rotation <- container.rotation - 0.01 * delta 
) |> ignore