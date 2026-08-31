import { useEffect, useState } from 'react'
import { codeToHtml } from 'shiki'

export function CodeBlock({ code }: { code: string }) {
  const [html, setHtml] = useState('')

  useEffect(() => {
    let cancelled = false
    setHtml('')
    codeToHtml(code, { lang: 'csharp', theme: 'github-light' })
      .then((result) => {
        if (!cancelled) {
          setHtml(result)
        }
      })
      .catch(() => {
        // Подсветка — не критичный путь: если shiki упал, показываем код без неё,
        // но не оставляем карточку пустой навсегда.
        if (!cancelled) {
          setHtml(`<pre class="p-3">${escapeHtml(code)}</pre>`)
        }
      })
    return () => {
      cancelled = true
    }
  }, [code])

  if (!html) {
    return (
      <div className="overflow-x-auto rounded-md border border-slate-200 p-3 text-sm text-slate-400">
        Подсветка кода…
      </div>
    )
  }

  return <div className="overflow-x-auto rounded-md border border-slate-200 text-sm" dangerouslySetInnerHTML={{ __html: html }} />
}

function escapeHtml(text: string): string {
  return text
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
}
