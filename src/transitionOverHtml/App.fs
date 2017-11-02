module Pixi

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.Animejs
open Fable.Pixi

let prepareSprite canvas (x,y,radius) container= 

  // create our texture from our canvas
  let texture = PIXI.Texture.fromCanvas canvas

  // prepare our sprite
  let sprite = 
    PIXI.Sprite texture
    |> SpriteUtils.setAnchor 0.5 0.5


  // update our texture from our canvas 
  // draw a circle with a hole inside
  let ctx = canvas.getContext_2d()
  ctx.beginPath()
  //ctx.arc(x,y,radius,startAngle,endAngle, anticlockwise);  
  ctx.arc(radius,radius,radius,0.,JS.Math.PI * 2., false) // outer (filled)
  ctx.arc(radius,radius,radius*0.1,0.,JS.Math.PI * 2., true) // outer (unfills it)
  ctx.fill()

  // update our sprite texture with newly drawn texture
  texture.update()

  // finish to prepare our sprite
  sprite
    |> SpriteUtils.moveTo x y
    |> SpriteUtils.scaleTo 0.5 0.5
    |> SpriteUtils.addToContainer container

// start our animation, which will just grow our circle to reveal a larger portion of the screen 
let startAnim (sprite:PIXI.Sprite) _ = 

  let scaleOut target duration scaleFactor= 

    let options = jsOptions<AnimInput> (fun o ->
      o.Item <- "x",scaleFactor
      o.Item <- "y",scaleFactor
      o.targets <- Some !!target
      o.duration <- !!duration
      o.easing <- !!EaseInOutElastic
      o.delay <- !!500.
    )
    let myAnim = Fable.AnimeUtils.GetInstance (Some options)
    myAnim.complete <-  fun _ -> 
      let div : HTMLDivElement = !!document.getElementById("title")
      div.innerText <- "We drew into a canvas and had this texture applied to a pixi sprite before animating it with animejs!"
  
  // hide our loader
  let div : HTMLDivElement = !!document.getElementById("loader")
  div.style.visibility <- "hidden"

  // show our starting content
  let div : HTMLDivElement = !!document.getElementById("content")
  div.style.visibility <- "visible"

  // start our scale animation
  scaleOut sprite.scale 2000. 2.|> ignore


let getCircleData (renderer:PIXI.WebGLRenderer) = 
  let x = renderer.width * 0.5
  let y = renderer.height * 0.5
  x,y

let prepareCanvas radius = 
  let canvas : HTMLCanvasElement = !!document.getElementById("canvas-layout")
  // set width and height to circle size 
  canvas.width  <- radius * 2.
  canvas.height <- radius * 2.
  // hide our canvas
  canvas.style.display <- "none"
  canvas

let start() = 
  let width = window.innerWidth
  let height = window.innerHeight

  // Let's create our pixi application
  let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
    o.antialias <- Some true
    o.transparent <- Some true
  )
  let app = PIXI.Application(width, height, options)

  // add our app to our div element
  let pixiCanvas : HTMLDivElement = !!document.getElementById("pixi-layout")
  pixiCanvas.appendChild(app.view) |> ignore

  // let's wait a little bit so we get this very big canvas texture added to our sprite
  let radius = width * 2.
  let (x,y) = (getCircleData !!app.renderer)
  let sprite = prepareSprite (prepareCanvas radius) (x,y,radius) app.stage
  window.setTimeout( (startAnim sprite), 1500) |> ignore

start()