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

  type DragonData =
    { 
      mutable Toggle: bool
      PrimaryTexture : PIXI.Texture
      SecondaryTexture : PIXI.Texture
    }
    
  // swap our texture on click
  let onTap (dragon: ExtendedSprite<DragonData>) _ =
    dragon.alpha <- 1.
    dragon.Data.Toggle <- not dragon.Data.Toggle
    if dragon.Data.Toggle then 
      dragon.texture <- dragon.Data.PrimaryTexture
    else
      dragon.texture <- dragon.Data.SecondaryTexture

  let make x y texture texture2 =
    
    // Extended Sprite from Fable.Pixi allows to add custom data inside our Sprite
    let dragon = ExtendedSprite(texture, { Toggle = true; PrimaryTexture= texture; SecondaryTexture = texture2 })

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
      |> attachEvent Pointertap (onTap dragon)


(* ------------------------ GAME ---------------------*)


// our view
let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

// add our Dragon !
let texture = PIXI.Texture.fromImage("../img/fable_logo.png")
let texture2 = PIXI.Texture.fromImage("../img/alfonso.png")

let renderer : PIXI.WebGLRenderer = !!app.renderer
let x = renderer.width * 0.5
let y = renderer.height * 0.5 

let dragon = 
  Dragon.make x y texture texture2
    |> app.stage.addChild
  

// stat our rotation
app.ticker.add(fun delta ->
  dragon.rotation <- dragon.rotation + 0.01
) |> ignore

  