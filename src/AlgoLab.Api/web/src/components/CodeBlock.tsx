import { useEffect, useState } from 'react'
import { createHighlighterCore, type HighlighterCore } from 'shiki/core'
import { createJavaScriptRegexEngine } from 'shiki/engine/javascript'
import csharp from 'shiki/langs/csharp.mjs'
import githubLight from 'shiki/themes/github-light.mjs'

// Fine-grained bundle: this app highlights exactly one language, so pulling in shiki's
// full bundle (every grammar it supports) would bloat the production build for no reason.
// The JS regex engine avoids an onig.wasm asset too. One shared highlighter, loaded once
// and reused across every CodeBlock instance.
let highlighterPromise: Promise<HighlighterCore> | null = null

function getHighlighter(): Promise<HighlighterCore> {
  highlighterPromise ??= createHighlighterCore({
    themes: [githubLight],
    langs: [csharp],
    engine: createJavaScriptRegexEngine(),
  })
  return highlighterPromise
}

export function CodeBlock({ code }: { code: string }) {
  const [html, setHtml] = useState('')

  useEffect(() => {
    let cancelled = false
    setHtml('')
    getHighlighter()
      .then((highlighter) => highlighter.codeToHtml(code, { lang: 'csharp', theme: 'github-light' }))
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
