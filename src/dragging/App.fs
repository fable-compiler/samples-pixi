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
let app = PIXI.Application(800., 600., options)
Browser.document.body.appendChild(app.view) |> ignore

let modes = createEmpty<PIXI.CONST.SCALE_MODESType>
PIXI.settings.Globals.SCALE_MODE <- modes.NEAREST

let texture = PIXI.Texture.fromImage("../img/fable_logo_small.png")

// since we d'ont have any {this} context, 
// we make a custom class to handle our events on bunnies
// full typed by the way!
type MyBunny(texture) =
  inherit PIXI.Sprite(texture)

  // store a simple dragging flag when me move the bunny
  let mutable isDragging = false
  
  // our event data
  let mutable data : PIXI.interaction.InteractionData option = None

  member this.onDragStart(event:PIXI.interaction.InteractionEvent) =
      // store a reference to the data
      // the reason for this is because of multitouch
      // we want to track the movement of this particular touch
      data <- Some event.data
      this.alpha <- 0.5
      isDragging <- true

  member this.onDragEnd() =
      data <- None
      this.alpha <- 1.
      isDragging <- false

  member this.onDragMove() =
    if isDragging then
      if( data.IsSome) then 
        let newPosition = data.Value.getLocalPosition(this.parent)
        let position : PIXI.Point = !!this.position
        position.x <-  newPosition.x
        position.y <- newPosition.y

  member this.noop() = console.log("click")
  member this.tap = this.noop
  member this.click = this.noop
  member this.mousedown = this.onDragStart
  member this.touchstart = this.onDragStart
  member this.touchend = this.onDragEnd
  member this.touchendoutside = this.onDragEnd
  member this.mouseupoutside = this.onDragEnd
  member this.mouseup = this.onDragEnd
  member this.touchmove = this.onDragMove
  member this.mousemove = this.onDragMove


let createBunny x y =
    // create our little bunny friend..
    let bunny = MyBunny(texture)

    // enable the bunny to be interactive... this will allow it to respond to mouse and touch events
    bunny.interactive <- true

    // this button mode will mean the hand cursor appears when you roll over the bunny with your mouse
    bunny.buttonMode <- true

    // center the bunny"s anchor point
    bunny.anchor.set(0.5)

    bunny.x <- x
    bunny.y <- y

    // add it to the stage
    app.stage.addChild(bunny) |> ignore


let renderer : PIXI.WebGLRenderer = !!app.renderer
for i in 0..9 do 
  createBunny 
    (Math.floor(Math.random() * renderer.width))
    (Math.floor(Math.random() * renderer.height))
  