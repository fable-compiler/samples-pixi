module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

let app = PIXI.Application()
Browser.document.body.appendChild(app.view) |> ignore

let texture = PIXI.Texture.fromImage("../img/p2.jpeg")

let renderer : PIXI.WebGLRenderer = !!app.renderer

let tilingSprite = 
  PIXI.extras.TilingSprite(
    texture, 
    renderer.width, 
    renderer.height
  )

app.stage.addChild tilingSprite |> ignore

let mutable count = 0.
let tick delta =
  count <- count + 0.005

  let scale : PIXI.Point = !!tilingSprite.tileScale
  scale.x <- 2. + Math.sin(count)
  scale.y <- 2. + Math.cos(count)

  let position : PIXI.Point = !!tilingSprite.tilePosition
  position.x <- position.x + 1.
  position.y <- position.y + 1.

// Listen for animate update
app.ticker.add(tick) |> ignore
