export type Verdict = 'Consistent' | 'Divergent' | 'Inconclusive'
export type RunStatus = 'Passed' | 'Failed' | 'NotImplemented' | 'Error'
export type SolutionKind = 'Reference' | 'Attempt'

export interface ProblemSummary {
  slug: string
  title: string
  difficulty: 'Easy' | 'Medium' | 'Hard'
  tags: string[]
  hasScaler: boolean
}

export interface SolutionView {
  id: string
  name: string
  kind: SolutionKind
  time: string
  space: string
  note: string | null
  source: string
}

export interface CaseView {
  name: string
  input: string
  expected: string
}

export interface ProblemDetail {
  summary: ProblemSummary
  source: string
  readme: string
  cases: CaseView[]
  solutions: SolutionView[]
}

export interface CaseResult {
  name: string
  input: string
  expected: string
  actual: string
  passed: boolean
  elapsedMs: number
  error: string | null
}

export interface RunResult {
  slug: string
  solutionId: string
  status: RunStatus
  cases: CaseResult[]
}

export interface ComplexityVerdict {
  declared: string
  bestFit: string
  declaredText: string
  bestFitText: string
  fitQuality: number
  kind: Verdict
}

export interface MeasurePoint {
  n: number
  medianMs: number
  allocatedBytes: number
}

export interface GrowthSeries {
  slug: string
  solutionId: string
  points: MeasurePoint[]
  time: ComplexityVerdict
  space: ComplexityVerdict
  error: string | null
}

export interface RunnerPayload {
  run: RunResult | null
  series: GrowthSeries[] | null
  error: string | null
}
