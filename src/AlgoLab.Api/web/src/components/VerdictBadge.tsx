import type { ComplexityVerdict } from '../api/types'

const styles = {
  Consistent: 'bg-emerald-50 text-emerald-800',
  Divergent: 'bg-red-50 text-red-800',
  Inconclusive: 'bg-slate-100 text-slate-600',
} as const

const words = {
  Consistent: 'согласуется',
  Divergent: 'расходится',
  Inconclusive: 'данных мало',
} as const

export function VerdictBadge({ verdict }: { verdict: ComplexityVerdict }) {
  return (
    <span className={`inline-block rounded px-2 py-1 text-xs ${styles[verdict.kind]}`}>
      заявлено {verdict.declaredText} · измерено {verdict.bestFitText} · {words[verdict.kind]}
    </span>
  )
}
