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
    { mutable IsDragging: bool
      mutable Interaction : PIXI.interaction.InteractionData option }  

  let onDragStart (dragon: ExtendedSprite<DragonData>) (ev:PIXI.interaction.InteractionEvent) = 
    dragon.alpha <- 0.5
    dragon.Data.Interaction <- Some ev.data
    dragon.Data.IsDragging <- true
      
  let onDragEnd (dragon: ExtendedSprite<DragonData>) _ =
    dragon.alpha <- 1.
    dragon.Data.Interaction <- None
    dragon.Data.IsDragging <- false

  let onDragMove (dragon: ExtendedSprite<DragonData>) _ =
    if dragon.Data.IsDragging then 
      if dragon.Data.Interaction.IsSome then 
        let interaction = dragon.Data.Interaction.Value
        let localPosition : PIXI.Point = interaction.getLocalPosition(dragon.parent) 
        let position : PIXI.Point = !!dragon.position      
        position.x <- localPosition.x
        position.y <- localPosition.y

  let make x y texture =
    
    // Extended Sprite from Fable.Pixi allows to add custom data inside our Sprite
    let dragon = ExtendedSprite(texture, { IsDragging = false; Interaction = None })

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

    // attach our custom events to enable dragging
    dragon
      // When attaching an anonymous lambda we capture the instance
      |> attachEvent Pointerdown (onDragStart dragon)
      // When attaching a module function we can use currrying
      |> attachEvent Pointerup (onDragEnd dragon)
      |> attachEvent Pointermove (onDragMove dragon)


(* ------------------------ GAME ---------------------*)


// our view
let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.backgroundColor <- Some 0x000000
)
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

// add our dragons !
let renderer : PIXI.WebGLRenderer = !!app.renderer
let texture = PIXI.Texture.fromImage("../img/fable_logo_small.png")
for i in 0..9 do 
  let randomX = (Math.floor(Math.random() * renderer.width))
  let randomY = (Math.floor(Math.random() * renderer.height)) 
  Dragon.make randomX randomY texture
    |> app.stage.addChild
    |> ignore
  