module Types 

open Fable.Import.Pixi
open Fable.Pixi

type Size = 
  | Tiny 
  | Small
  | Medium 
  | Large

type CogData = {
  Size: Size
  mutable Target: float*float
  mutable StartPosition: float*float
  mutable IsDragging: bool
  mutable Interaction : PIXI.interaction.InteractionData option    
  mutable IsFound: bool
}

type Msg =
  | OnMove of ExtendedSprite<CogData>

type State = 
  | Init
  | PlaceCogs
  | PlaceDock
  | Play 
  | GameOver 
  | DoNothing

type Model = {
  Cogs : ExtendedSprite<CogData> []
  Targets : ExtendedSprite<CogData> []
  mutable Message : Msg option
  mutable Score : int
  Goal : int
  State : State
  mutable Found: int []
}

let mutable model = { 
    Cogs = [||] 
    Targets = [||]
    Message = None
    Score= 0
    Goal=0
    State=Init
    Found=[||]
  }