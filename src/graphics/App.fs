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
  o.antialias <- Some true
)

let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let graphics =  PIXI.Graphics()

// set a fill and line style
graphics.beginFill(0xFF3300)
  .lineStyle(4., 0xffd900, 1.)
  .moveTo(50.,50.)
  .lineTo(250., 50.)
  .lineTo(100., 100.)
  .lineTo(50., 50.)
  .endFill()

// set a fill and a line style again and draw a rectangle
  .lineStyle(2., 0x0000FF, 1.)
  .beginFill(0xFF700B, 1.)
  .drawRect(50., 250., 120., 120.) 

// draw a rounded rectangle
  .lineStyle(2., 0xFF00FF, 1.)
  .beginFill(0xFF00BB, 0.25)
  .drawRoundedRect(150., 450., 300., 100., 15.)
  .endFill() 

// draw a circle, set the lineStyle to zero so the circle doesn't have an outline
  .lineStyle(0.)
  .beginFill(0xFFFF0B, 0.5)
  .drawCircle(470., 90.,60.)
  .endFill() |> ignore

app.stage.addChild(graphics) |> ignore
