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
    o._fontFamily<- !^"Arial"
    o._fontSize<- !^36.
    o._fontStyle<- "italic"
    o._fontWeight<- "bold"
    o._fill<- [|"#ffffff";"#00ff99"|] // gradient
    o._stroke<- !^"#4a1850"
    o._strokeThickness<- 5.
    o._dropShadow<- true
    o._dropShadowColor<- !^"#000000"
    o._dropShadowBlur<- 4.
    o._dropShadowAngle<- Math.PI / 6.
    o._dropShadowDistance<- 6.
    o._wordWrap<- true
    o._wordWrapWidth<- 440.
)

let richText = PIXI.Text("Rich text with a lot of options and across multiple lines",style)
richText.x <- 30.
richText.y <- 180.

app.stage.addChild richText |> ignore

(*
var basicText = new PIXI.Text('Basic text in pixi');
basicText.x = 30;
basicText.y = 90;

app.stage.addChild(basicText);

var style = new PIXI.TextStyle({
    fontFamily: 'Arial',
    fontSize: 36,
    fontStyle: 'italic',
    fontWeight: 'bold',
    fill: ['#ffffff', '#00ff99'], // gradient
    stroke: '#4a1850',
    strokeThickness: 5,
    dropShadow: true,
    dropShadowColor: '#000000',
    dropShadowBlur: 4,
    dropShadowAngle: Math.PI / 6,
    dropShadowDistance: 6,
    wordWrap: true,
    wordWrapWidth: 440
});

var richText = new PIXI.Text('Rich text with a lot of options and across multiple lines', style);
richText.x = 30;
richText.y = 180;

app.stage.addChild(richText);
*)