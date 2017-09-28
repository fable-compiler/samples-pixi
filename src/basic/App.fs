module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

// create a new Sprite from an image path
//let options = PIXI.ApplicationOptions.BackgroundColor 0x1099bb 
let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0xAAAACC

let app = PIXI.Application(400., 400., options)
Browser.document.body.appendChild(app.view) |> ignore

// create a new Sprite from an image path
let bunny = PIXI.Sprite.fromImage("../img/fable_logo_small.png")

let renderer : PIXI.WebGLRenderer = !!app.renderer

// center the sprite's anchor point
bunny.anchor.set(0.5)
bunny.x <- renderer.width * 0.5
bunny.y <- renderer.height * 0.5

app.stage.addChild(bunny) |> ignore

let tick delta =
  // just for fun, let's rotate mr rabbit a little
  // delta is 1 if running at 100% performance
  // creates frame-independent tranformation
  bunny.rotation <- bunny.rotation + 0.1 * delta

// Listen for animate update
app.ticker.add(tick) |> ignore
