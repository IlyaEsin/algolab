import type { ProblemDetail, ProblemSummary, RunnerPayload } from './types'

async function get<T>(url: string): Promise<T> {
  const response = await fetch(url)
  if (!response.ok) {
    throw new Error(`Запрос ${url} вернул ${response.status}`)
  }
  return response.json() as Promise<T>
}

async function post<T>(url: string, body: unknown): Promise<T> {
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!response.ok) {
    throw new Error(`Запрос ${url} вернул ${response.status}`)
  }
  return response.json() as Promise<T>
}

export const api = {
  listProblems: () => get<ProblemSummary[]>('/api/problems'),
  getProblem: (slug: string) => get<ProblemDetail>(`/api/problems/${slug}`),
  run: (slug: string, solutionId: string) => post<RunnerPayload>('/api/run', { slug, solutionId }),
  measure: (slug: string, solutionIds: string[]) => post<RunnerPayload>('/api/measure', { slug, solutionIds }),
}
