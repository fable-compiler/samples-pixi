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
let app = PIXI.Application(800.,600.,options)
Browser.document.body.appendChild(app.view) |> ignore

let mutable count = 0.

// build a rope!
let ropeLength = 45.

let points = 
  [|
    for i in 0..24 do 
      yield PIXI.Point(float i * ropeLength, 0.)
  |]

let strip = new PIXI.mesh.Rope(PIXI.Texture.fromImage("../img/snake.png"), !!points)

strip.x <- -40.
strip.y <- 300.

app.stage.addChild(strip) |> ignore

let g = PIXI.Graphics()
g.x <- strip.x
g.y <- strip.y
app.stage.addChild(g) |> ignore

let renderPoints () =
    g
      .clear()
      .lineStyle(2.,0xffc2c2)
      .moveTo(points.[0].x,points.[0].y) |> ignore

    for i in 0..points.Length - 1 do 
        g.lineTo(points.[i].x,points.[i].y) |> ignore
    
    for i in 0..points.Length - 1 do 
        g.beginFill(0xff0022)
          .drawCircle(points.[i].x,points.[i].y,10.) 
          .endFill() |> ignore
    
// start animating
app.ticker.add(fun delta ->

    count <- count + 0.1

    // make the snake
    for i in 0..points.Length - 1 do 
      points.[i].y <- Math.sin((float i * 0.5) + count) * 30.
      points.[i].x <- float i * ropeLength + Math.cos((float i * 0.3) + count) * 20.
    
    renderPoints()
) |> ignore

