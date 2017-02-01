import Immutable from 'immutable';

const prefix = 'LAYOUT.';

// Select perspective, page, tab, browser location
export const SELECT_LOCATION = prefix + 'SELECT_LOCATION';
export function setPerspective(perspective, page, tab, preview) {
	let action = { type: SELECT_LOCATION, perspective };
	if (page) {
		action.page = page;
		if (tab) {
			action.tab = tab;
			if (preview) {
				action.preview = preview;
			}
		}
	}
	return action;
}
export function setPage(page, tab, preview) {
	let action = { type: SELECT_LOCATION, page };
	if (tab) {
		action.tab = tab;
		if (preview) {
			action.preview = preview;
		}
	}
	return action;
}
export function setTab(tab, preview) {
	let action = { type: SELECT_LOCATION, tab };
	if (preview) {
		action.preview = preview;
	}
	return action;
}
export function setPreview(preview) {
	return { type: SELECT_LOCATION, preview };
}

// Open page
export const OPEN_PAGE = prefix + 'OPEN_PAGE';
export function openPage(pageName, tabNames) {
	return { type: OPEN_PAGE, pageName, tabNames };
}

// Close page
export const CLOSE_PAGE = prefix + 'CLOSE_PAGE';

// open/close perspective explorer
export const TOGGLE_EXPLORER = prefix + 'TOGGLE_EXPLORER';
export function toggleExplorer() {
	return {type: TOGGLE_EXPLORER };
}

function capitalizeFirstLetter(string) {
	return string.charAt(0).toUpperCase() + string.slice(1);
}
// set value on layout object
export const SET_VALUE = prefix + 'SET_VALUE';
const pageLevels = ['perspective', 'page', 'tab'];
export function setValue(level, key, value) {
	let levelIndex = pageLevels.indexOf(level);
	if (levelIndex === -1) {
		level = pageLevels[0];
		levelIndex = 0;
	}
	let levelUp = pageLevels[levelIndex + 1];
	if (levelUp && (key === levelUp + 's' || key === 'current' + capitalizeFirstLetter(levelUp))) {
		return {
			type: SET_VALUE
		};
	}
	return {
		type: SET_VALUE,
		level,
		key,
		value
	};
}

// Show seo, dev, log, help
// Hide seo, dev, log, help
// Move split seo, dev, log, help

const initialState = Immutable.Map({
	currentPerspective: 'content',
	perspectivesOpen: false,
	perspectives: Immutable.OrderedMap({
		content: Immutable.Map(),
		media: Immutable.Map(),
		data: Immutable.Map(),
		layout: Immutable.Map(),
		functions: Immutable.Map(),
		system: Immutable.Map()
	})
});

export function digChangeAndReturnLayout(levelIndex, key, value, currentObject, currentIndex = -1) {
	if (levelIndex === currentIndex) {
		return currentObject.set(key, value);
	} else {
		let newIndex = currentIndex + 1;
		let name = currentObject.get('current' + capitalizeFirstLetter(pageLevels[newIndex]));
		let newObject = currentObject.getIn([pageLevels[newIndex] + 's', name]) || Immutable.Map;
		return currentObject.setIn([pageLevels[newIndex] + 's', name], digChangeAndReturnLayout(levelIndex, key, value, newObject, newIndex));
	}
}

export default function layout(state = initialState, action) {
	switch (action.type) {
	case SELECT_LOCATION:
		return state.withMutations(state => {
			let perspective = state.getIn(['perspectives', action.perspective || state.get('currentPerspective')]);
			if (perspective) {
				if (action.perspective) {
					state.set('currentPerspective', action.perspective);
				}
				state.setIn(['perspectives', state.get('currentPerspective')], perspective.withMutations(perspective => {
					let page = perspective.getIn(['pages', action.page || perspective.get('currentPage')]);
					if (page) {
						if (action.page) {
							perspective.set('currentPage', action.page);
						}
						perspective.setIn(['pages', perspective.get('currentPage')], page.withMutations(page => {
							let tab = page.getIn(['tabs', action.tab || page.get('currentTab')]);
							if (tab) {
								if (action.tab) {
									page.set('currentTab', action.tab);
								}
								if (action.preview) {
									page.setIn(['tabs', page.get('currentTab')], tab.withMutations(tab => {
										tab.set('previewLocation', action.preview);
									}));
								}
							}
						}));
					}
				}));
			}
		});
	case OPEN_PAGE:
		if(state.getIn(['perspectives', state.get('currentPerspective'), 'pages', action.pageName])) return state;
		return state.setIn(
			['perspectives', state.get('currentPerspective'), 'pages', action.pageName],
			Immutable.Map({
				tabs: (action.tabNames || []).reduce((tabs, tabName) =>
					tabs.set(tabName, Immutable.Map()), Immutable.OrderedMap())
			})
		);
	case CLOSE_PAGE:
		return state.withMutations(state => {
			if (state.getIn(['perspectives', state.get('currentPerspective'), 'currentPage']) === action.pageName) {
				state.setIn(
					['perspectives', state.get('currentPerspective'), 'currentPage'],
					state.getIn(['perspectives', state.get('currentPerspective'), 'pages']).keySeq().first()
				);
			}
			state.deleteIn([
				'perspectives',
				state.get('currentPerspective'),
				'pages',
				action.pageName
			]);
		});
	case TOGGLE_EXPLORER:
		return state.set('perspectivesOpen', !state.get('perspectivesOpen'));
	case SET_VALUE:
		return digChangeAndReturnLayout(pageLevels.indexOf(action.level), action.key, action.value, state);
	default:
		return state;
	}
}
