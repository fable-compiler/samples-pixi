module Fable.AnimeUtils

open Fable.Import.Animejs
open Fable.Core.JsInterop

(*
let XY (target:'t) xFactor yFactor duration elasticity =
  jsOptions<AnimatableProperties> (fun o ->
    o.elasticity <- !!elasticity
    o.duration <- !!duration
    o.targets <- !!target
    o.Item("x") <- xFactor
    o.Item("y") <- yFactor
  )

let SingleParameter (target:'t) parameterName paramaterFactor duration elasticity =
  jsOptions<anime.AnimeAnimParams> (fun o ->
    o.elasticity <- !!elasticity
    o.duration <- !!duration
    o.targets <- !!target
    o.Item(parameterName) <- paramaterFactor
  )
*)

let GetInstance (options:AnimInput) =
  let instance = Globals.anime
  instance.Invoke options

let GetTimeline (options:AnimationParameters) =
  let instance = Globals.anime
  instance.timeline options
