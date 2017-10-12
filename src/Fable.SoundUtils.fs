module Fable.SoundUtils 

open Fable.Import.Pixi.Sound

let play name = 
  let hasSound = PIXI.sound.Globals.exists name
  if hasSound then 
    PIXI.sound.Globals.play(name) |> ignore
  else 
    failwith (sprintf "unknown sound %s" name)
