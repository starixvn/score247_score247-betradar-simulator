project = 'Score247'
copyright = '2019, Starixsoft Company'
author = 'Starixsoft Company'

version = ''
release = ''

extensions = [
    'sphinx.ext.autodoc',
    'sphinx.ext.doctest',
    'sphinx.ext.intersphinx',
    'sphinx.ext.todo',
    'sphinx.ext.coverage',
    'sphinx.ext.mathjax',
    'sphinx.ext.autosectionlabel'
]
templates_path = ['_templates']
source_suffix = '.rst'
master_doc = 'index'
language = None
exclude_patterns = ['_build', 'Thumbs.db', '.DS_Store']
pygments_style = 'sphinx'
html_theme = 'sphinx_rtd_theme'
html_theme_options = {
    'canonical_url': '',
    'analytics_id': '',
    'logo_only': False,
    'display_version': True,
    'prev_next_buttons_location': 'bottom',
    # 'style_external_links': False,
    # Toc options
    'collapse_navigation': True,
    'sticky_navigation': True,
    'navigation_depth': 4
    # 'includehidden': True,
    # 'titles_only': False
}

html_context = {
    "display_gitlab": True,  # Integrate Gitlab
    "gitlab_host": "gitlab.nexdev.net",
    "gitlab_user": "livescore",  # Username
    "gitlab_repo": "LiveScoreApp",  # Repo name
    "gitlab_version": "master",  # Version
    "conf_py_path": "/docs/",  # Path in the checkout to the docs root
}
html_static_path = ['_static']
htmlhelp_basename = 'Score247'
latex_elements = {
}

latex_documents = [
    (master_doc, 'Score247.tex', 'Score247 Documentation',
     'Starix Soft', 'manual'),
]

man_pages = [
    (master_doc, 'Score247', 'Score247 Documentation',
     [author], 1)
]

texinfo_documents = [
    (master_doc, 'Score247', 'Score247 Documentation',
     author, 'Score247', 'One line description of project.',
     'Miscellaneous'),
]
epub_title = project
epub_author = author
epub_publisher = author
epub_copyright = copyright
epub_exclude_files = ['search.html']
intersphinx_mapping = {'https://docs.python.org/': None}
todo_include_todos = True
