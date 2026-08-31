import { useCallback, useEffect, useState } from 'react'

const key = (slug: string) => `algolab.reveal.${slug}`

/**
 * Раскрытие эталонов держится в localStorage: сценарий «сначала решить сам»
 * ломается, если перезагрузка страницы показывает решение сама.
 *
 * ProblemPage не перемонтируется при смене slug (меняется только пропс), поэтому
 * инициализатор useState здесь отработал бы один раз и застрял бы на первой задаче —
 * эффект ниже перечитывает localStorage при каждой смене slug, иначе можно
 * незаметно унести revealed=true с прошлой задачи на новую.
 */
export function useReveal(slug: string) {
  const [revealed, setRevealed] = useState(() => localStorage.getItem(key(slug)) === '1')

  useEffect(() => {
    setRevealed(localStorage.getItem(key(slug)) === '1')
  }, [slug])

  const reveal = useCallback(() => {
    localStorage.setItem(key(slug), '1')
    setRevealed(true)
  }, [slug])

  const hide = useCallback(() => {
    localStorage.removeItem(key(slug))
    setRevealed(false)
  }, [slug])

  return { revealed, reveal, hide }
}

const solvedKey = (slug: string) => `algolab.solved.${slug}`

/**
 * Отметка «решено» ставится браузером после успешного прогона попытки.
 * Сервер её не считает: чтобы узнать это на стороне API, пришлось бы исполнять
 * попытку на каждый запрос списка — а зациклившаяся попытка повесила бы список.
 */
export function isSolved(slug: string): boolean {
  return localStorage.getItem(solvedKey(slug)) === '1'
}

export function markSolved(slug: string): void {
  localStorage.setItem(solvedKey(slug), '1')
}
