module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

// create a new Sprite from an image path
let options = createEmpty<PIXI.ApplicationOptions>
options.backgroundColor <- Some 0xAAAACC

let app = PIXI.Application(400., 400., options)
Browser.document.body.appendChild(app.view) |> ignore

let onAssetsLoaded() = 

  let frames = 
    [|
      for i in 0..29 do
        let index = if i < 10 then 0 else i
        let name = sprintf "rollSequence00%i.png" index
        let texture = PIXI.Texture.fromFrame name
        yield 
          texture
    |]

  let anim = PIXI.extras.AnimatedSprite !!frames
  let renderer : PIXI.WebGLRenderer = !!app.renderer
  anim.x <- renderer.width / 2.
  anim.y <- renderer.height / 2.
  anim.anchor.set(0.5)
  anim.animationSpeed <- 0.5
  anim.play()

  app.stage.addChild anim |> ignore
  app.ticker.add( fun delta -> anim.rotation <- anim.rotation + 0.01 ) |> ignore

let loader = PIXI.loaders.Loader("../img/fighter.json")
loader.load(onAssetsLoaded) |> ignore

(*
PIXI.loader
    .add('required/assets/basics/fighter.json')
    .load(onAssetsLoaded);

function onAssetsLoaded()
{
    // create an array of textures from an image path
    var frames = [];

    for (var i = 0; i < 30; i++) {
        var val = i < 10 ? '0' + i : i;

        // magically works since the spritesheet was loaded with the pixi loader
        frames.push(PIXI.Texture.fromFrame('rollSequence00' + val + '.png'));
    }

    // create an AnimatedSprite (brings back memories from the days of Flash, right ?)
    var anim = new PIXI.extras.AnimatedSprite(frames);

    /*
     * An AnimatedSprite inherits all the properties of a PIXI sprite
     * so you can change its position, its anchor, mask it, etc
     */
    anim.x = app.renderer.width / 2;
    anim.y = app.renderer.height / 2;
    anim.anchor.set(0.5);
    anim.animationSpeed = 0.5;
    anim.play();

    app.stage.addChild(anim);

    // Animate the rotation
    app.ticker.add(function() {
        anim.rotation += 0.01;
    });
}
  
*)
