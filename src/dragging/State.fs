module State

open Fable.Import

let [<Literal>] DRAGON_PARAMS = 2
let [<Literal>] DRAGON_COUNT = 10

// For simplicity, we use these values here but
// state logic shouldn't know about renderer implementation
// See `stateToRenderCoordinates` in Renderer.fs
let [<Literal>] RENDERER_WIDTH = 800.
let [<Literal>] RENDERER_HEIGHT = 600.

type Dragons = float[]

type StateModel =
  | Dragging of idx:int * Dragons
  | Waiting of Dragons

// For convenience we use same set events for the two states
type Event =
  | Pointerdown of idx:int
  | Pointermove of idx:int * x:float * y:float
  | Pointerup of idx:int

let setDragonXY idx x y (dragons: Dragons) =
  let pos = idx * DRAGON_PARAMS
  Array.set dragons pos x
  Array.set dragons (pos + 1) y

let getDragonXY idx (dragons: Dragons) =
  let pos = idx * DRAGON_PARAMS
  dragons.[pos], dragons.[pos + 1]

let getDragonCount (dragons: Dragons) =
  Array.length dragons / DRAGON_PARAMS

let updateDragging (evs: Event seq) (model: StateModel): StateModel =
  (model, evs) ||> Seq.fold (fun model ev ->
    match model, ev with
    | Dragging(idx1, dragons), Pointermove(idx2, x, y) when idx1 = idx2 ->
      setDragonXY idx1 x y dragons
      model
    | Dragging(idx1, dragons), Pointerup idx2 when idx1 = idx2 ->
      Waiting dragons
    // Ignore other combinations
    | _ -> model)

let updateWaiting (evs: Event seq) (model: StateModel): StateModel =
  (model, evs) ||> Seq.fold (fun model ev ->
    match model, ev with
    | Waiting dragons, Pointerdown(idx) ->
      Dragging(idx, dragons)
    // Ignore other combinations
    | _ -> model)

let initState(): StateModel =
  let dragons = Array.zeroCreate<float> DRAGON_COUNT
  for idx = 0 to DRAGON_COUNT - 1 do
    let randomX = (JS.Math.floor(JS.Math.random() * RENDERER_WIDTH))
    let randomY = (JS.Math.floor(JS.Math.random() * RENDERER_HEIGHT))
    setDragonXY idx randomX randomY dragons
  Waiting dragons
