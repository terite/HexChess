using System.Diagnostics;

namespace Dretch
{
    public class DiagnosticInfo
    {
        public readonly Stopwatch moveGenTimer = new Stopwatch();
        public readonly Stopwatch moveSortTimer = new Stopwatch();
        public readonly Stopwatch moveValidateTimer = new Stopwatch();
        public readonly Stopwatch evalTimer = new Stopwatch();
        public readonly Stopwatch evalThreatsTimer = new Stopwatch();
        public readonly Stopwatch applyTimer = new Stopwatch();
        public readonly Stopwatch getMoveTimer = new Stopwatch();

        public int boardEvaluations = 0;
        public int terminalBoardEvaluations = 0;
        public int invalidMoves = 0;

        public void Reset()
        {
            moveGenTimer.Reset();
            moveSortTimer.Reset();
            moveValidateTimer.Reset();
            evalTimer.Reset();
            evalThreatsTimer.Reset();
            applyTimer.Reset();
            getMoveTimer.Restart();
            boardEvaluations = 0;
            invalidMoves = 0;
        }

        public Measurer MeasureMoveGen() => new Measurer(moveGenTimer);
        public Measurer MeasureMoveSort() => new Measurer(moveSortTimer);
        public Measurer MeasureMoveValidate() => new Measurer(moveValidateTimer);
        public Measurer MeasureEval() => new Measurer(evalTimer);
        public Measurer MeasureEvalThreats() => new Measurer(evalThreatsTimer);
        public Measurer MeasureApply() => new Measurer(applyTimer);
        public Measurer MeasureGetMove() => new Measurer(getMoveTimer);
    }
}
