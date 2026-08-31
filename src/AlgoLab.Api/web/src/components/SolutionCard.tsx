import type { SolutionView } from '../api/types'
import { CodeBlock } from './CodeBlock'
import { RunPanel } from './RunPanel'

interface Props {
  slug: string
  solution: SolutionView
  revealed: boolean
  onReveal: () => void
}

export function SolutionCard({ slug, solution, revealed, onReveal }: Props) {
  const hidden = solution.kind === 'Reference' && !revealed

  return (
    <section className="mb-4 rounded-lg border border-slate-200 p-4">
      <header className="mb-2 flex items-baseline gap-3">
        <h3 className="font-medium">{solution.name}</h3>
        <span className="text-xs text-slate-500">
          время {solution.time} · память {solution.space}
        </span>
      </header>
      {solution.note && <p className="mb-2 text-sm text-slate-600">{solution.note}</p>}
      {hidden ? (
        <button type="button" onClick={onReveal} className="rounded bg-slate-800 px-3 py-1 text-sm text-white">
          Показать решение
        </button>
      ) : (
        <CodeBlock code={solution.source} />
      )}
      <RunPanel slug={slug} solutionId={solution.id} kind={solution.kind} />
    </section>
  )
}
