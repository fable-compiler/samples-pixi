module Types 

open Fable.Import.Pixi

open Fable.Import.Pixi.Sound
open Fable.Import.Animejs
open Fable.Import.Pixi.Particles
open Fable.Pixi
open Fable.Core.JsInterop


module GameScreen = 

  module Cog =
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

    type PointerId = int
    type Msg =
      | OnMove of ExtendedSprite<CogData> * PointerId

  type CogState = 
    | Init
    | PlaceHelp
    | PlaceCogs
    | PlaceDock
    | Play 
    | DoNothing
    | Win
    | MoveToNextScreen 

  type CogModel = {
    mutable Cogs : ExtendedSprite<Cog.CogData> []
    mutable Targets : ExtendedSprite<Cog.CogData> []
    mutable Score : int
    mutable Goal : int
    mutable Found: int []
    mutable State: CogState
    mutable Emitters : PIXI.particles.Emitter []
    Sizes : Cog.Size []
    Layers: string list
  }

[<RequireQualifiedAccess>]
module IntroductionScreen = 

  type State = 
    | Init 
    | Play 
    | EndAnim 
    | MoveToNextScreen 
    | ByeBye
    | DoNothing

  type Texts = PIXI.Sprite *  PIXI.Sprite

  type CustomSprite = {
    Angle:float
  }

  type Model = {
    mutable State : State
    mutable Texts : Texts option
    Layers: string list
    mutable CogList: ExtendedSprite<CustomSprite> []
    mutable Id: float
  }

type ScreenKind = 
  | GameOfCogs of GameScreen.CogModel option
  | GameOver  
  | Title  of IntroductionScreen.Model option
  | NextScreen of ScreenKind

