module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Sound
open Fable.Import.Browser
open Fable.Import.JS


let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
)

let app = PIXI.Application(400., 400., options)
Browser.document.body.appendChild(app.view) |> ignore

// kind of pretty scale mode
let modes = createEmpty<PIXI.CONST.SCALE_MODESType>
PIXI.settings.Globals.SCALE_MODE <- modes.NEAREST

let renderer : PIXI.WebGLRenderer = !!app.renderer

// our dragon
let sprite = PIXI.Sprite.fromImage("../img/fable_logo_small.png")
sprite.anchor.set(0.5)
sprite.x <- renderer.width * 0.5
sprite.y <- renderer.height * 0.5
sprite.interactive <- true
sprite.buttonMode <- true
app.stage.addChild sprite |> ignore 

// our label
let style = jsOptions<PIXI.TextStyle>( fun o -> 
    o.fontFamily<- !^"Arial"
    o.fontSize<- !^36.
    o.fontStyle<- "italic"
    o.fontWeight<- "bold"
    o.fill<- [|"#ffffff";"#1292FF"|] // gradient
    o.strokeThickness<- 5.
)
let basicText = PIXI.Text("Click on the dragon!",style)
basicText.anchor.set(0.5)
basicText.x <- renderer.width * 0.5
basicText.y <- 90.
app.stage.addChild basicText |> ignore

// our sound
let sound = PIXI.sound.Sound.from(!!"../sounds/applause.ogg")

sprite.on("pointerdown", fun _ -> 
  let scale : PIXI.Point = !!sprite.scale
  scale.x <- scale.x * 1.25 
  scale.y <- scale.y * 1.25 

  // play the sound when user clicks on the dragon
  sound.play() |> ignore
) |> ignore