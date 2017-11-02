module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.Animejs
open Fable.AnimeUtils

let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.antialias <- Some true
  o.transparent <- Some true
)

let app = PIXI.Application(800., 600., options)
let renderer: PIXI.WebGLRenderer = !!app.renderer
Browser.document.body.appendChild(app.view) |> ignore

let graphics =  PIXI.Graphics()

let x = renderer.width * 0.5
let y = renderer.height * 0.5
// set a fill and line style
graphics
// draw a circle, set the lineStyle to zero so the circle doesn't have an outline
  .lineStyle(0.)
  .beginFill(0x000000, 0.9)
  .drawCircle(x, y, 500.)
  .endFill() |> ignore

let fadeInOut duration = 

  let options = jsOptions<AnimInput> (fun o ->
    o.Item <- "x",1.0
    o.Item <- "y",1.0
    o.targets <- Some !!target
    o.duration <- !!duration
    o.elasticity <- !!500.
    o.easing <- !!EaseInElastic
  )
  let instance = Fable.AnimeUtils.GetInstance options
  instance.complete <-
    fun _ ->
      let options =
        jsOptions<AnimInput> (fun o ->
          o.Item <- "x",0.
          o.Item <- "y",0.
          o.targets <- Some !!target
          o.duration <- !!duration
          o.elasticity <- !!0.
          o.easing <- !!EaseInQuad
          o.delay <- !!400.)
      Fable.AnimeUtils.GetInstance options |> ignore

graphics.scale
app.stage.addChild(graphics) |> ignore
