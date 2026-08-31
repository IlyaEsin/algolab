import { useMemo, useState } from 'react'
import type { ProblemSummary } from '../api/types'
import { isSolved } from '../state/useReveal'

interface Props {
  problems: ProblemSummary[]
  selected: string | null
  onSelect: (slug: string) => void
}

export function ProblemList({ problems, selected, onSelect }: Props) {
  const [query, setQuery] = useState('')
  const [tag, setTag] = useState('')

  const tags = useMemo(
    () => [...new Set(problems.flatMap((p) => p.tags))].sort(),
    [problems],
  )

  const visible = problems.filter(
    (p) =>
      p.title.toLowerCase().includes(query.toLowerCase()) &&
      (tag === '' || p.tags.includes(tag)),
  )

  return (
    <aside className="w-72 shrink-0 border-r border-slate-200 p-4">
      <input
        className="mb-2 w-full rounded border border-slate-300 px-2 py-1 text-sm"
        placeholder="Поиск"
        value={query}
        onChange={(e) => setQuery(e.target.value)}
      />
      <select
        className="mb-4 w-full rounded border border-slate-300 px-2 py-1 text-sm"
        value={tag}
        onChange={(e) => setTag(e.target.value)}
      >
        <option value="">все теги</option>
        {tags.map((t) => (
          <option key={t} value={t}>
            {t}
          </option>
        ))}
      </select>
      <ul>
        {visible.map((p) => (
          <li key={p.slug}>
            <button
              type="button"
              onClick={() => onSelect(p.slug)}
              className={`w-full rounded px-2 py-1 text-left text-sm ${p.slug === selected ? 'bg-slate-200' : 'hover:bg-slate-100'}`}
            >
              <span>{p.title}</span>
              {isSolved(p.slug) && <span className="ml-2 text-emerald-600">✓</span>}
              <span className="block text-xs text-slate-500">{p.difficulty}</span>
            </button>
          </li>
        ))}
      </ul>
    </aside>
  )
}
