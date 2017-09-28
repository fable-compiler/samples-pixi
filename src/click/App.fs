module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0xAAAACC

let app = PIXI.Application(400., 400., options)
Browser.document.body.appendChild(app.view) |> ignore

let modes = createEmpty<PIXI.CONST.SCALE_MODESType>
PIXI.settings.Globals.SCALE_MODE <- modes.NEAREST

let sprite = PIXI.Sprite.fromImage("../img/fable_logo_small.png")

let renderer : PIXI.WebGLRenderer = !!app.renderer

sprite.anchor.set(0.5)
sprite.x <- renderer.width * 0.5
sprite.y <- renderer.height * 0.5

sprite.interactive <- true
sprite.buttonMode <- true

app.stage.addChild sprite |> ignore 

sprite.on("pointerdown", fun _ -> 
  let scale : PIXI.Point = !!sprite.scale
  scale.x <- scale.x * 1.25 
  scale.y <- scale.y * 1.25 
) |> ignore