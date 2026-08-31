import Markdown from 'react-markdown'

export function Statement({ markdown }: { markdown: string }) {
  return (
    <div className="prose prose-slate max-w-none text-sm">
      <Markdown>{markdown}</Markdown>
    </div>
  )
}
