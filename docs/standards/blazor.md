# Blazor

- One component per file. Component file name = component name.
- Feature-first folders. A page, its state, its service and its private components live together.
  Promote to `Shared/` only on the **second** consumer — one consumer means it stays inside the
  feature.
- A `Components/` folder nested inside a feature holds that feature's private components. The
  location **is** the access modifier.
- Navigate using the injected navigation manager's base URI, never a literal `"/"`. The app is
  served from a sub-path on static hosting, and a literal root path escapes the app entirely.
- `HttpClient.BaseAddress` ends with `/`; request URIs do **not** start with `/`.
- Anything holding a timer or a subscription implements `IDisposable` / `IAsyncDisposable` and
  actually disposes. A periodic timer left running leaks one loop per navigation.
- Every interactive element gets an accessible name and visible focus. ARIA and `:focus-visible`
  are written as the component is written, never retrofitted — retrofitting accessibility costs
  2–3× and is why it is built in from the start.
- Tap targets are at least 44×44 px; body text is at least 16 px on mobile. Code blocks scroll
  horizontally **inside their container**, never at page level.
- Use pointer events, not mouse-only handlers, so ordering and matching work on touch. Any
  drag interaction also needs a keyboard path.
- CSS is ours — **no framework, ever.** No Bootstrap, no Tailwind, no utility-first CDN. One
  stylesheet is the single source of style truth. Theming through CSS custom properties on
  `:root`, switched by `data-*` attributes on `<html>`.
- Route segments are Polish — see [naming-and-comments.md](naming-and-comments.md).
- Never render question content as raw HTML — see [security.md](security.md).
