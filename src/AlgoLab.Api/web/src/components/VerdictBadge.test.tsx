import { render, screen } from '@testing-library/react'
import { expect, test } from 'vitest'
import { VerdictBadge } from './VerdictBadge'

test('consistent verdict names both classes', () => {
  render(
    <VerdictBadge
      verdict={{ declared: 'ON', bestFit: 'ON', declaredText: 'O(n)', bestFitText: 'O(n)', fitQuality: 0.99, kind: 'Consistent' }}
    />,
  )

  expect(screen.getByText(/заявлено O\(n\)/)).toBeInTheDocument()
  expect(screen.getByText(/согласуется/)).toBeInTheDocument()
})

test('divergent verdict shows the measured class', () => {
  render(
    <VerdictBadge
      verdict={{ declared: 'ON', bestFit: 'ON2', declaredText: 'O(n)', bestFitText: 'O(n^2)', fitQuality: 0.97, kind: 'Divergent' }}
    />,
  )

  expect(screen.getByText(/измерено O\(n\^2\)/)).toBeInTheDocument()
})

test('inconclusive verdict says so instead of claiming a match', () => {
  render(
    <VerdictBadge
      verdict={{ declared: 'ON', bestFit: 'ON', declaredText: 'O(n)', bestFitText: 'O(n)', fitQuality: 0.4, kind: 'Inconclusive' }}
    />,
  )

  expect(screen.getByText(/данных мало/)).toBeInTheDocument()
})
