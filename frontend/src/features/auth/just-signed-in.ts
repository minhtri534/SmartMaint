/*
 * Remembers, for one page change, that the user has just signed in, so the app can fade in from the sign-in page's
 * night colour. Kept in memory on purpose: a reload of the app must not play the fade again.
 */
let justSignedIn = false

export const markJustSignedIn = () => {
  justSignedIn = true
}

export const wasJustSignedIn = () => justSignedIn

export const clearJustSignedIn = () => {
  justSignedIn = false
}
