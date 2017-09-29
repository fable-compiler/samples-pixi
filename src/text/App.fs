module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x1099bb
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let renderer : PIXI.WebGLRenderer = !!app.renderer

let basicText = PIXI.Text("Basic text in pixi")
basicText.x <- 30.
basicText.y <- 90.

app.stage.addChild basicText |> ignore

let style = jsOptions<PIXI.TextStyle>( fun o -> 
    o.fontFamily<- !^"Arial"
    o.fontSize<- !^36.
    o.fontStyle<- "italic"
    o.fontWeight<- "bold"
    o.fill<- [|"#ffffff";"#00ff99"|] // gradient
    o.stroke<- !^"#4a1850"
    o.strokeThickness<- 5.
    o.dropShadow<- true
    o.dropShadowColor<- !^"#000000"
    o.dropShadowBlur<- 4.
    o.dropShadowAngle<- Math.PI / 6.
    o.dropShadowDistance<- 6.
    o.wordWrap<- true
    o.wordWrapWidth<- 440.
)

let richText = PIXI.Text("Rich text with a lot of options and across multiple lines",style)
richText.x <- 30.
richText.y <- 180.

app.stage.addChild richText |> ignore