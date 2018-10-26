module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
)
let app = PIXI.Application(400., 400., options)
Browser.document.body.appendChild(app.view) |> ignore

// create a new Sprite from an image path
let bunny = PIXI.Sprite.fromImage("../img/fable_logo_small.png")

let renderer : PIXI.WebGLRenderer = !!app.renderer

// center the sprite's anchor point
bunny.anchor.set(0.5)
bunny.x <- app.screen.width * 0.5
bunny.y <- app.screen.height * 0.5

app.stage.addChild(bunny) |> ignore

let tick delta =
  // just for fun, let's rotate mr rabbit a little
  // delta is 1 if running at 100% performance
  // creates frame-independent tranformation
  bunny.rotation <- bunny.rotation + 0.1 * delta

// Listen for animate update
app.ticker.add(tick) |> ignore
