module Fable.AnimeUtils

open Fable.Import.Animejs
open Fable.Core.JsInterop

let XY (target:'t) xFactor yFactor duration elasticity =
  jsOptions<AnimInput> (fun o ->
    o.elasticity <- !!elasticity
    o.duration <- !!duration
    o.targets <- !!target
    o.Item("x") <- xFactor
    o.Item("y") <- yFactor
  )

let SingleParameter (target:'t) parameterName paramaterFactor duration elasticity =
  jsOptions<AnimInput> (fun o ->
    o.elasticity <- !!elasticity
    o.duration <- !!duration
    o.targets <- !!target
    o.Item(parameterName) <- paramaterFactor
  )

let GetInstance (options:AnimInput option) =
  match options with 
  | Some options -> 
    anime.Invoke options
  | None -> 
    anime.Invoke()

let GetTimeline (options:AnimInput option) =
  match options with 
  | Some options -> 
    anime.timeline options
  | None -> 
    anime.timeline()
