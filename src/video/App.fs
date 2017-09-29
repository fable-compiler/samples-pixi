module Pixi

open System
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Browser
open Fable.Import.JS

let options = jsOptions<PIXI.ApplicationOptions> (fun o ->
  o.transparent <- Some true
  o.backgroundColor <- Some 0x00000
)
let app = PIXI.Application(800.,600.,options)
Browser.document.body.appendChild(app.view) |> ignore

// Create play button that can be used to trigger the video
let button = 
  PIXI.Graphics()
    .beginFill(0x0, 0.5)
    .drawRoundedRect(0., 0., 100., 100., 10.)
    .endFill()
    .beginFill(0xffffff)
    .moveTo(36., 30.)
    .lineTo(36., 70.)
    .lineTo(70., 50.)

// Position the button
let renderer : PIXI.WebGLRenderer = !!app.renderer
button.x <- (renderer.width - button.width) / 2.
button.y <- (renderer.height - button.height) / 2.

// Enable interactivity on the button
button.interactive <- true
button.buttonMode <- true

// Add to the stage
app.stage.addChild(button) |> ignore

// Listen for a click/tap event to start playing the video
// this is useful for some mobile platforms. For example:
// ios9 and under cannot render videos in PIXI without a 
// polyfill - https://github.com/bfred-it/iphone-inline-video
// ios10 and above require a click/tap event to render videos 
// that contain audio in PIXI. Videos with no audio track do 
// not have this requirement
let onPlayVideo() =

    // Don"t need the button anymore
    button.destroy()

    // create a video texture from a path
    let texture = PIXI.Texture.fromVideo( !^"../img/testVideo.mp4")

    // create a  Sprite using the video texture (yes it"s that easy)
    let videoSprite = PIXI.Sprite(texture)

    // Stetch the fullscreen
    videoSprite.width <- renderer.width
    videoSprite.height <- renderer.height

    app.stage.addChild(videoSprite) |> ignore


button.on("pointertap", onPlayVideo) |> ignore



(*
var app = new PIXI.Application(800, 600, { transparent: true });
document.body.appendChild(app.view);

// Create play button that can be used to trigger the video
var button = new PIXI.Graphics()
    .beginFill(0x0, 0.5)
    .drawRoundedRect(0, 0, 100, 100, 10)
    .endFill()
    .beginFill(0xffffff)
    .moveTo(36, 30)
    .lineTo(36, 70)
    .lineTo(70, 50);

// Position the button
button.x = (app.renderer.width - button.width) / 2;
button.y = (app.renderer.height - button.height) / 2;

// Enable interactivity on the button
button.interactive = true;
button.buttonMode = true;

// Add to the stage
app.stage.addChild(button);

// Listen for a click/tap event to start playing the video
// this is useful for some mobile platforms. For example:
// ios9 and under cannot render videos in PIXI without a 
// polyfill - https://github.com/bfred-it/iphone-inline-video
// ios10 and above require a click/tap event to render videos 
// that contain audio in PIXI. Videos with no audio track do 
// not have this requirement
button.on('pointertap', onPlayVideo);

function onPlayVideo() {

    // Don't need the button anymore
    button.destroy();

    // create a video texture from a path
    var texture = PIXI.Texture.fromVideo('required/assets/testVideo.mp4');

    // create a new Sprite using the video texture (yes it's that easy)
    var videoSprite = new PIXI.Sprite(texture);

    // Stetch the fullscreen
    videoSprite.width = app.renderer.width;
    videoSprite.height = app.renderer.height;

    app.stage.addChild(videoSprite); 
}
*)