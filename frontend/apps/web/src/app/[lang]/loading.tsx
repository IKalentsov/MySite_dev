/**
 * The loading state of the home route: a skeleton in the place of the content, with
 * the same widths and block heights as the real page, so the first paint does not
 * shift once the data arrives.
 */
export default function Loading() {
  return (
    <div
      aria-busy="true"
      className="mx-auto flex min-h-svh w-full max-w-3xl flex-col px-6 py-10"
    >
      <div aria-hidden="true" className="flex flex-wrap items-center justify-between gap-3 border-b border-border pb-4">
        <div className="h-7 w-24 animate-pulse rounded bg-muted" />
        <div className="flex gap-3">
          <div className="h-4 w-14 animate-pulse rounded bg-muted" />
          <div className="h-4 w-14 animate-pulse rounded bg-muted" />
        </div>
      </div>

      <main aria-hidden="true" className="flex flex-col">
        <section className="mt-8">
          <div className="h-6 w-32 animate-pulse rounded bg-muted" />
          <div className="mt-3 h-5 w-2/3 animate-pulse rounded bg-muted" />
          <div className="mt-2 h-4 w-full animate-pulse rounded bg-muted" />
          <div className="mt-2 h-4 w-5/6 animate-pulse rounded bg-muted" />
        </section>

        <section className="mt-8">
          <div className="h-6 w-28 animate-pulse rounded bg-muted" />
          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            <div className="h-36 animate-pulse rounded border border-border bg-card" />
            <div className="h-36 animate-pulse rounded border border-border bg-card" />
          </div>
        </section>

        <section className="mt-10 border-t border-border pt-4">
          <div className="h-6 w-28 animate-pulse rounded bg-muted" />
          <div className="mt-3 flex gap-4">
            <div className="h-4 w-20 animate-pulse rounded bg-muted" />
            <div className="h-4 w-16 animate-pulse rounded bg-muted" />
          </div>
        </section>
      </main>
    </div>
  );
}
