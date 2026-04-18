---
name: timevic-content
description: Create and refine TimeVic website content. Use for landing page copy, hero sections, headings, subheadings, CTA text, FAQ, public pages, onboarding copy, SEO metadata, empty states, product value propositions, and content audits for time tracking, projects, payments, reports, tasks, and team productivity pages.
---

# TimeVic Content

## Overview

Use this skill to draft product and public-site content for TimeVic. Content should be clear, concrete, and useful for teams that track time, manage projects and tasks, review reports, and connect work with payments.

## Voice

- Write in plain, direct language.
- Prefer concrete product outcomes over broad productivity claims.
- Avoid hype, filler, and vague enterprise language.
- Do not claim certifications, integrations, security guarantees, pricing, customer counts, or performance numbers unless they are present in the source material.
- Use active voice and short sentences.
- Keep copy suitable for small teams, agencies, freelancers, and internal project teams unless a different audience is specified.

## Page Workflow

1. Identify the page goal: acquisition, activation, support, trust, or conversion.
2. Identify the primary audience and their current problem.
3. List the real product capabilities that support the page promise.
4. Draft the page in sections: hero, proof or feature blocks, workflow, objections, FAQ, CTA.
5. Keep the H1 focused on the product, offer, or page category.
6. Put detailed value propositions in supporting copy, not an overloaded headline.
7. Make CTA labels specific: `Start tracking time`, `Try demo workspace`, `View reports`, `Create workspace`.
8. Review for unsupported claims and remove anything that cannot be backed by the product or provided source.

## Landing Page Sections

Use these sections when they fit the request:

- Hero: one clear promise, one short paragraph, one primary CTA, optional secondary CTA.
- Time tracking: start and stop timers, edit entries, review active work.
- Projects and tasks: connect tracked time to clients, projects, task lists, and work items.
- Reports: summarize time by date, week, month, user, project, client, or payment context when supported.
- Team workflow: workspace membership, access, and collaboration benefits without overstating admin features.
- Integrations: mention ClickUp, Redmine, Jira, SMTP, Firebase, or other integrations only when relevant to the actual page.
- FAQ: answer objections about setup, reports, team use, corrections, and data ownership in practical terms.
- CTA: repeat the next action with a short reason to act.

## Headline Rules

- Keep H1 direct and literal.
- Use headings that describe the section content, not abstract slogans.
- Avoid puns unless the user explicitly asks for a playful tone.
- Avoid overusing "all-in-one", "seamless", "revolutionize", "unlock", and "supercharge".
- Prefer "Track time across projects" over "Transform the way your team works".

## FAQ Rules

- Write questions in the user's language.
- Answer directly in the first sentence.
- Keep each answer to 2-4 sentences unless the topic is legal, billing, or setup-heavy.
- Do not invent policy, retention, compliance, or pricing details.
- Include implementation notes separately from final copy when a developer needs to wire content into UI.

## Public UI Copy

- Empty states should explain what is missing and provide the next action.
- Error copy should be specific enough to help recovery but should not expose internal details.
- Form labels should be short and literal.
- Button labels should be verbs or verb phrases.
- Toast messages should confirm the completed action or explain the failed action.

## Output Format

When drafting content, provide copy in a structure that can be pasted into the implementation:

```markdown
## Hero
H1: ...
Subheading: ...
Primary CTA: ...
Secondary CTA: ...

## FAQ
Q: ...
A: ...
```

If the user asks to implement the content in Razor components, use `timevic-frontend` after the copy is approved or clearly implied.
