[<RequireQualifiedAccess>]
module Elmish.Animation.Program

open Elmish
open Fable.Import

let [<Literal>] MAX_TIMESTEP = 100.

let inline private requestFrame f =
    Browser.window.requestAnimationFrame(Browser.FrameRequestCallback f) |> ignore

let withAnimation
    (tick : float -> 'model -> 'model)
    (pauseAnimation: (unit->unit)->unit)
    (program:Elmish.Program<_,'model,'msg,_>) =

    let mutable model = Unchecked.defaultof<'model>
    let mutable animating = true

    let rec animate dispatch last t =
        if animating then
            // Make sure the time delta is not too big (can happen if user switches browser tab)
            let timestep = min MAX_TIMESTEP (t - last)
            // TODO: Wrap this in a try..catch
            model <- tick timestep model
            program.view model dispatch
            requestFrame (animate dispatch t)

    let init arg =
        let subscribeAnimation dispatch =
            requestFrame (animate dispatch 0.)
        let subscribeAnimationPause dispatch =
            let toggle() =
                animating <- not animating
                if animating then
                    requestFrame (animate dispatch 0.)
            pauseAnimation toggle
        let model, cmd = program.init arg
        model, cmd @ [subscribeAnimation; subscribeAnimationPause]

    let setState m dispatch =
        model <- m
        if not animating then
            program.view model dispatch

    { program with init = init; setState = setState }