module Fable.Pixi

open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.JS
open Fable.Import.Browser
open Fable.Import.Pixi
open Fable.Core.JsInterop

type ExtendedSprite<'T> (texture:PIXI.Texture,data: 'T) =
  inherit PIXI.Sprite(texture)
  member this.Data = data

type EventHandler = PIXI.interaction.InteractionEvent->unit

[<StringEnum>]
type PixiEvent =
  | Pointerdown
  | Pointerup  
  | Pointerupoutside
  | Pointermove  

let attachEvent (ev: PixiEvent) (handler: EventHandler) (sprite: PIXI.Sprite) =
  sprite.on(!!(string ev), handler) |> ignore
  sprite
