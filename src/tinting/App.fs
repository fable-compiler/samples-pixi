module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS
open Fable.Pixi

(* ------------------------ DRAGON ---------------------*)

module Dragon = 
  
  type DragonData = {
    mutable Tint: float
  }

  // swap our texture on click
  let onTap (dragon: ExtendedSprite<DragonData>) _ =
    dragon.Data.Tint <- Math.random() * (float 0xFFFFFF) // not really needed unless we do something important from this value
    dragon.tint <- dragon.Data.Tint

  let make x y texture =
    
    // Extended Sprite from Fable.Pixi allows to add custom data inside our Sprite
    let dragon = ExtendedSprite(texture,{Tint=1.0})

      // enable the dragon to be interactive... this will allow it to respond to mouse and touch events
    dragon.interactive <- true

      // this button mode will mean the hand cursor appears when you roll over the dragon with your mouse
    dragon.buttonMode <- true

    // center the dragon"s anchor point
    dragon.anchor.set(0.5)

    // place our dragon on screen
    let position : PIXI.Point = !!dragon.position
    position.x <- x
    position.y <- y

    dragon
      |> Fable.Pixi.Event.attach Fable.Pixi.Event.Pointertap (onTap dragon)


(* ------------------------ GAME ---------------------*)


// our view
let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

// our label
let renderer : PIXI.WebGLRenderer = !!app.renderer
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

// add our Dragon !
let texture = PIXI.Texture.fromImage("../img/fable_logo.png")

let x = renderer.width * 0.5
let y = renderer.height * 0.5 

let scale (scalefactor:float) (sprite:PIXI.Sprite) = 
  let scale : PIXI.Point = !!sprite.scale
  scale.x <- scale.x * scalefactor
  scale.y <- scale.y * scalefactor
  sprite

let dragon = 
  Dragon.make x y texture 
    |> app.stage.addChild
    |> scale 0.5
  
// stat our rotation
app.ticker.add(fun delta ->
  dragon.rotation <- dragon.rotation + 0.01
  basicText.tint <- dragon.tint
) |> ignore

  