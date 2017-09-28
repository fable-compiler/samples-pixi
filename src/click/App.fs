module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

// create a new Sprite from an image path
//let options = PIXI.ApplicationOptions.BackgroundColor 0x1099bb 
let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0xAAAACC

let app = PIXI.Application(400., 400., options)
Browser.document.body.appendChild(app.view) |> ignore

let modes = createEmpty<PIXI.CONST.SCALE_MODESType>
PIXI.settings.Globals.SCALE_MODE <- modes.NEAREST

let sprite = PIXI.Sprite.fromImage("../img/fable_logo_small.png")

let renderer : PIXI.WebGLRenderer = !!app.renderer

sprite.anchor.set(0.5)
sprite.x <- renderer.width * 0.5
sprite.y <- renderer.height * 0.5

sprite.interactive <- true
sprite.buttonMode <- true

app.stage.addChild sprite |> ignore 

sprite.on("pointerdown", fun _ -> 
  let scale : PIXI.Point = !!sprite.scale
  scale.x <- scale.x * 1.25 
  scale.y <- scale.y * 1.25 
) |> ignore

(*
  // Scale mode for all textures, will retain pixelation
PIXI.settings.SCALE_MODE = PIXI.SCALE_MODES.NEAREST;

var sprite = PIXI.Sprite.fromImage('required/assets/basics/bunny.png');

// Set the initial position
sprite.anchor.set(0.5);
sprite.x = app.renderer.width / 2;
sprite.y = app.renderer.height / 2;

// Opt-in to interactivity
sprite.interactive = true;

// Shows hand cursor
sprite.buttonMode = true;

// Pointers normalize touch and mouse
sprite.on('pointerdown', onClick);

// Alternatively, use the mouse & touch events:
// sprite.on('click', onClick); // mouse-only
// sprite.on('tap', onClick); // touch-only

app.stage.addChild(sprite);

function onClick () {
    sprite.scale.x *= 1.25;
    sprite.scale.y *= 1.25;
}
*)
