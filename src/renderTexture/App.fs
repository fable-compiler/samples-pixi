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
let app = PIXI.Application(800.,600.,options)
Browser.document.body.appendChild(app.view) |> ignore

let container = PIXI.Container()
app.stage.addChild(container) |> ignore

let texture = PIXI.Texture.fromImage("../img/bunny.png")

for i in 0..24 do
  let bunny = PIXI.Sprite(texture)
  bunny.x <- (float i % 5.) * 30.
  bunny.y <- Math.floor(float i / 5.) * 30.
  bunny.rotation <- Math.random() * (Math.PI * 2.)
  container.addChild(bunny) |> ignore

let modes = createEmpty<PIXI.CONST.SCALE_MODESType>
let brt = PIXI.BaseRenderTexture(300., 300., modes.LINEAR, 1.)
let rt = PIXI.RenderTexture(brt)

let sprite = PIXI.Sprite(rt)

sprite.x <- 450.
sprite.y <- 60.
app.stage.addChild(sprite) |> ignore

(*
 * All the bunnies are added to the container with the addChild method
 * when you do this, all the bunnies become children of the container, and when a container moves,
 * so do all its children.
 * This gives you a lot of flexibility and makes it easier to position elements on the screen
 *)
container.x <- 100.
container.y <- 60.

let renderer : PIXI.WebGLRenderer = !!app.renderer
app.ticker.add( fun delta -> 
    renderer.render(container, rt)
) |> ignore
