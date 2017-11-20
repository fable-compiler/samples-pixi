module Renderer

open State
open Fable.Import
open Fable.Import.Pixi
open Fable.Core.JsInterop
open Fable.Pixi

module Values =
  let [<Literal>] BACKGROUND_COLOR = 0x000000
  let [<Literal>] APP_WIDTH = 800.
  let [<Literal>] APP_HEIGHT = 600.

open Values

type RenderModel =
  { dragging: int option
    dragons: PIXI.Sprite[] }

let stateToRenderCoordinates (x, y) =
  x, y // TODO: Transform coordinates

let onPointerdown idx dispatch _ =
  Pointerdown(idx) |> dispatch

let onPointerup idx dispatch _ =
  Pointerup idx |> dispatch

let onPointermove idx dispatch (dragon:PIXI.Sprite) (ev:PIXI.interaction.InteractionEvent) =
  let localPosition : PIXI.Point = ev.data.getLocalPosition(dragon.parent)
  Pointermove(idx, localPosition.x, localPosition.y) |> dispatch

let makeDragon idx x y texture dispatch =
  let dragon = PIXI.Sprite(texture)
  // enable the dragon to be interactive... this will allow it to respond to mouse and touch events
  dragon.interactive <- true
  // this button mode will mean the hand cursor appears when you roll over the dragon with your mouse
  dragon.buttonMode <- true
  // center the dragon's anchor point
  dragon.anchor.set(0.5)
  // place our dragon on screen
  let position : PIXI.Point = !!dragon.position
  position.x <- x
  position.y <- y
  dragon
  |> Event.attach Event.Pointerdown (onPointerdown idx dispatch)
  |> Event.attach Event.Pointerup (onPointerup idx dispatch)
  |> Event.attach Event.Pointermove (onPointermove idx dispatch dragon)

let initModel (smodel: StateModel) dispatch =
  let app = PIXI.Application(APP_WIDTH, APP_HEIGHT, jsOptions(fun o ->
    o.backgroundColor <- Some BACKGROUND_COLOR))
  Browser.document.body.appendChild(app.view) |> ignore
  let dragons =
    match smodel with
    | Dragging(_,dragons)
    | Waiting dragons -> dragons
  let dragonCount = getDragonCount dragons
  let texture = PIXI.Texture.fromImage("../img/fable_logo_small.png")
  { dragging = None
    dragons =
      Array.init dragonCount (fun idx ->
        let x, y =
          getDragonXY idx dragons
          |> stateToRenderCoordinates
        makeDragon idx x y texture dispatch
        |> app.stage.addChild)
  }

let changeAlpha (draggingIdx: int option) (rmodel: RenderModel) =
    match draggingIdx, rmodel.dragging with
    // Dragging, change to transparent
    | Some _, Some _ -> rmodel
    | Some idx, None ->
      let dragon = Array.item idx rmodel.dragons
      dragon.alpha <- 0.5
      { rmodel with dragging = Some idx }
    // Stopped dragging, remove transparency
    | None, None -> rmodel
    | None, Some idx ->
      let dragon = Array.item idx rmodel.dragons
      dragon.alpha <- 1.
      { rmodel with dragging = None }

let render (smodel: StateModel) (rmodel: RenderModel option) dispatch: RenderModel =
  let rmodel =
    match rmodel with
    | Some rmodel -> rmodel
    | None -> initModel smodel dispatch
  match smodel with
  | Waiting _ ->
    changeAlpha None rmodel
  | Dragging(idx, sdragons) ->
    let dragon = Array.item idx rmodel.dragons
    let position: PIXI.Point = !!dragon.position
    let x, y =
      getDragonXY idx sdragons
      |> stateToRenderCoordinates
    position.x <- x
    position.y <- y
    changeAlpha (Some idx) rmodel
