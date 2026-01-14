# Design Guidelines: AI Documentation Assistant Chatbot

## Design Approach
**System-Based with Chat Pattern Specialization**: Drawing from Linear's clean aesthetic and Notion's content clarity, combined with modern chat interface patterns from Slack and Discord. Focus on readability, information hierarchy, and frictionless interaction.

## Color Palette
**Calming & Professional Theme**:
- Primary: Soft indigo (#6366F1) for accents and CTAs
- Background: Off-white (#FAFBFC) with subtle warmth
- Surface: Pure white (#FFFFFF) for message cards
- AI Response Background: Light lavender (#F5F3FF) 
- User Message Background: Soft blue (#EFF6FF)
- Text Primary: Charcoal (#1F2937)
- Text Secondary: Medium gray (#6B7280)
- Borders: Whisper gray (#E5E7EB)
- Code Background: Slate (#F8FAFC) with blue tint
- Success/Interactive: Emerald (#10B981)

## Typography
- **Primary Font**: Inter (Google Fonts) - exceptional legibility
- **Code Font**: Fira Code (Google Fonts) - programming ligatures
- **Scale**:
  - H1 (Welcome/Empty State): text-3xl font-semibold (30px)
  - H2 (Section Headers): text-xl font-semibold (20px)
  - Body (Messages): text-base (16px)
  - Small (Timestamps): text-sm text-gray-500 (14px)
  - Code: text-sm font-mono (14px)

## Layout System
**Spacing Framework**: Tailwind units of 2, 4, 6, 8, 12, 16
- Component padding: p-4 to p-6
- Section gaps: gap-4 to gap-8
- Generous whitespace: py-12 for main container

**Structure**:
```
Main Container (max-w-4xl mx-auto)
├── Header (h-16, fixed top)
│   └── Logo + Model Selector + New Chat Button
├── Messages Area (flex-1, overflow-y-auto, px-4 py-8)
│   └── Message Cards (mb-6 spacing)
└── Input Zone (sticky bottom, p-4 backdrop-blur)
    ├── Suggestion Chips (scrollable horizontal)
    └── Input Bar + Send Button
```

## Core Components

### Message Bubbles
- **User Messages**: Right-aligned, max-w-2xl, rounded-2xl, bg-blue-50, p-4, shadow-sm
- **AI Messages**: Left-aligned, max-w-3xl, rounded-2xl, bg-purple-50, p-6, shadow-md
- **Spacing**: mb-6 between messages, mb-2 for timestamp
- **Avatar**: 8x8 rounded-full icon (user) or AI logo (assistant)

### Input Area
- **Container**: Sticky bottom-0, bg-white/80 backdrop-blur-lg, border-t
- **Text Input**: Min-h-12, max-h-32 (auto-resize), rounded-xl border-2, focus:border-indigo-500, px-4 py-3, transition-all
- **Send Button**: Absolute right-2, rounded-lg, bg-indigo-600 hover:bg-indigo-700, p-2.5
- **Placeholder**: "Ask about our documentation..." with subtle animation

### Suggestion Chips
- **Container**: Horizontal scroll (hide scrollbar), gap-2, mb-4
- **Chip Style**: Inline-flex, px-4 py-2, rounded-full, bg-white border-2 border-gray-200, hover:border-indigo-300, hover:bg-indigo-50, transition-colors, cursor-pointer
- **Content**: Icon (16px) + Text (text-sm), gap-2

### Documentation Rendering
- **Headers**: font-semibold with mb-3 spacing
- **Code Blocks**: bg-slate-50, rounded-lg, p-4, overflow-x-auto, syntax highlighting (Prism.js)
- **Inline Code**: bg-slate-100, px-1.5 py-0.5, rounded, font-mono text-sm
- **Lists**: ml-5, my-2, list-disc (ul) or list-decimal (ol)
- **Copy Button**: Absolute top-2 right-2 on code blocks, opacity-0 hover:opacity-100

### Loading States
- **Typing Indicator**: Three dots animation, in AI message bubble style
- **Skeleton**: Pulse animation for message placeholders

## Mobile Responsiveness
- **Breakpoints**: 
  - Mobile: < 640px (single column, full width)
  - Tablet: 640px - 1024px (max-w-3xl)
  - Desktop: > 1024px (max-w-4xl)
- **Mobile Adjustments**:
  - Reduce padding: p-3 instead of p-6
  - Smaller text: text-sm for body
  - Stack suggestion chips vertically if space constrained
  - Input bar: min-h-10, text-sm
  - Fixed header with hamburger menu for settings

## Interaction & Accessibility
- **Focus States**: 2px indigo ring, rounded to match element
- **ARIA Labels**: All interactive elements properly labeled
- **Keyboard Navigation**: Tab through chips, Enter to send, Shift+Enter for new line
- **Touch Targets**: Minimum 44x44px for mobile
- **Animations**: Subtle fade-in (200ms) for messages, smooth scroll behavior

## Key Patterns
- Empty state: Centered, large heading "How can I help?" with 4-6 suggestion chips below
- Error states: Gentle red accent (#EF4444) with retry option
- Session persistence indicator: Small "Saved" badge with checkmark
- Message timestamps: Right-aligned, text-xs, text-gray-400, mt-1

This design prioritizes clarity, reduces cognitive load, and creates a serene environment for focused documentation exploration.